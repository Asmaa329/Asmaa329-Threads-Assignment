using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class OrderValidator
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly CouponRepository _couponRepository;
        private readonly OrderRepository _orderRepository;

        public OrderValidator(
            CustomerRepository customerRepository,
            ProductRepository productRepository,
            CouponRepository couponRepository,
            OrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _couponRepository = couponRepository;
            _orderRepository = orderRepository;
        }
        public async Task ValidateOrderAsync(Order order, CancellationToken ct)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId,ct);
            if (customer==null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Customer #{order.CustomerId} not found";
                await _orderRepository.SaveAsync(order, ct);
                return;
            }
            if(!customer.IsActive)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = $"Customer #{order.CustomerId} not found";
                await _orderRepository.SaveAsync(order, ct);
                return;
            }
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId,ct);

                if (product == null)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Product #{item.ProductId} not found";
                        await _orderRepository.SaveAsync(order, ct);
                    return;
                }
                if (product.StockQuantity < item.Quantity)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Insufficient stock for '{product.Name}'";
                    await _orderRepository.SaveAsync(order, ct);
                    return;
                }
            }
            if (!string.IsNullOrEmpty(order.CouponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(order.CouponCode,ct);
                if (coupon == null)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Coupon '{order.CouponCode}' does not exist";
                    await _orderRepository.SaveAsync(order, ct);
                    return;
                }
                if (coupon.ExpiryDate < DateTime.Today)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason = $"Coupon '{order.CouponCode}' has expired";
                    await _orderRepository.SaveAsync(order, ct);
                    return;
                }
            }
            order.Status = OrderStatus.Validated;
            order.FailureReason = null; 
            await _orderRepository.SaveAsync(order, ct );
        }
    }
    
}
