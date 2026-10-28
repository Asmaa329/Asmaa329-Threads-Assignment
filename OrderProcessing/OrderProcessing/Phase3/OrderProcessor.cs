using OrderProcessing.OrderProcessing.Phase2;
using OrderProcessing.OrderProcessing.Phase3;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing
{
    public class OrderProcessor
    {
        private readonly CustomerService _customerService;
        private readonly InventoryService _inventoryService;
        private readonly PricingService _pricingService;
        private readonly OrderPricer _orderPricer;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IEmailService _emailService;
        private readonly JsonFileStore _store = new();
        private readonly HashSet<int> _failSaveOrderIds;

        public OrderProcessor(
            CustomerService customerService,
            InventoryService inventoryService,
            PricingService pricingService,
            OrderPricer orderPricer,
            IPaymentGateway paymentGateway,
            IEmailService emailService,
            IEnumerable<int> failSaveOrderIds)
        {
            _customerService = customerService;
            _inventoryService = inventoryService;
            _pricingService = pricingService;
            _orderPricer = orderPricer;
            _paymentGateway = paymentGateway;
            _emailService = emailService;
            _failSaveOrderIds = new HashSet<int>(failSaveOrderIds);
        }

        public async Task<bool> ProcessOrderAsync(Order order, CancellationToken ct)
        {
            Customer customer;
            try
            {
                customer = await _customerService.GetCustomerAsync(order.CustomerId, ct);
                if (!customer.IsActive)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = "Customer is inactive";
                    await SaveOrderAsync(order, ct);
                    return false;
                }
            }
            catch (Exception ex)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = ex.Message;
                await SaveOrderAsync(order, ct);
                return false;
            }

            bool stockReserved = await _inventoryService.ReserveStockAsync(order, ct);
            if (!stockReserved)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = "Insufficient stock"; 
                await SaveOrderAsync(order, ct);
                return false;
            }

            try
            {
                Coupon? coupon = null;
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    var coupons = await _store.ReadAsync<List<Coupon>>(DataFiles.Coupons, ct);
                    coupon = coupons.FirstOrDefault(c => c.Code.Equals(order.CouponCode, StringComparison.OrdinalIgnoreCase));
                }
                await _orderPricer.PriceOrderAsync(order,ct);
            }
            catch (Exception ex)
            {
                await _inventoryService.ReleaseStockAsync(order, ct);
                order.Status = OrderStatus.Rejected;
                order.FailureReason = ex.Message;
                await SaveOrderAsync(order, ct);
                return false;
            }

            PaymentResult paymentResult;
            try
            {
                using var paymentCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                paymentCts.CancelAfter(3000); 

                paymentResult = await _paymentGateway.ProcessPaymentAsync(order.Id, order.TotalAmount, paymentCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                await _inventoryService.ReleaseStockAsync(order, ct);
                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = "Payment gateway timeout (3s)";
                await SaveOrderAsync(order, ct);
                return false;
            }
            catch (Exception)
            {
                await _inventoryService.ReleaseStockAsync(order, ct);
                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = "Payment gateway error";
                await SaveOrderAsync(order, ct);
                return false;
            }

            var paymentRecord = new PaymentTransaction
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Status = paymentResult.Status,
                TransactionId = paymentResult.TransactionId,
                CreatedAt = DateTime.Now
            };
            var payments = await _store.ReadAsync<List<PaymentTransaction>>(DataFiles.Payments, ct);
            payments.Add(paymentRecord);
            await _store.WriteAsync(DataFiles.Payments, payments, ct);

            if (!paymentResult.Success)
            {
                await _inventoryService.ReleaseStockAsync(order, ct);
                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = paymentResult.Message;
                await SaveOrderAsync(order, ct);
                return false;
            }

            order.TransactionId = paymentResult.TransactionId;
            order.Status = OrderStatus.Completed;

            try
            {
                if (_failSaveOrderIds.Contains(order.Id))
                {
                    throw new Exception("Connection lost during commit");
                }
                await SaveOrderAsync(order, ct);
            }
            catch (Exception)
            {
                await _paymentGateway.RefundAsync(order.TransactionId!, ct);
                await _inventoryService.ReleaseStockAsync(order, ct);
                order.Status = OrderStatus.Failed;
                order.FailureReason = $"Database save failed after payment; payment {order.TransactionId} refunded";
                await SaveOrderAsync(order, ct);
                return false;
            }

            await _emailService.SendConfirmationWithRetryAsync(order, customer, ct);

            return order.Status == OrderStatus.Completed;
        }

        private async Task SaveOrderAsync(Order order, CancellationToken ct)
        {
            var orders = await _store.ReadAsync<List<Order>>(DataFiles.Orders, ct);
            var existing = orders.FirstOrDefault(o => o.Id == order.Id);
            if (existing != null)
            {
                orders.Remove(existing);
            }
            orders.Add(order);
            await _store.WriteAsync(DataFiles.Orders, orders, ct);
        }
    }
}
