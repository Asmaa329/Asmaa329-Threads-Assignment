using OrderProcessing.OrderProcessing.Phase1;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace OrderProcessing.OrderProcessing.Phase2
{
    public class PricingService
    {
        private readonly OrderRepository _orderRepository;
        private readonly ProductRepository _productRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly CouponRepository _couponRepository;
        private readonly IEnumerable<int> _failShippingForOrderIds;

        public PricingService(OrderRepository orderRepository, ProductRepository productRepository, CustomerRepository customerRepository, CouponRepository couponRepository, IEnumerable<int> failShippingForOrderIds)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _couponRepository = couponRepository;
            _failShippingForOrderIds = failShippingForOrderIds;
        }

        public async Task<decimal> CalculateSubTotalAsync(Order order, CancellationToken cancellationToken = default)
        {
            decimal subTotal = 0;

            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);

                if (product != null)
                {
                    item.UnitPrice = product.Price; 
                    subTotal += item.Quantity * product.Price;
                }
            }
            order.SubTotal = Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
            return order.SubTotal;
        }

        public async Task<decimal> GetDiscountAsync(Order order, CancellationToken ct)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId, ct);
            decimal discountPercent = 0;

            if (customer != null)
            {
                discountPercent = customer.LoyaltyTier switch
                {
                    LoyaltyTier.Bronze => 0,
                    LoyaltyTier.Silver => 5,
                    LoyaltyTier.Gold => 10,
                    _ => 0
                };
            }
            if (!string.IsNullOrEmpty(order.CouponCode))
            {
                var coupon = await _couponRepository.GetByCodeAsync(order.CouponCode, ct);
                if (coupon != null && coupon.ExpiryDate >= DateTime.Today)
                {
                    discountPercent += coupon.DiscountPercent;
                }
            }
            if (discountPercent > 30)
            {
                discountPercent = 30;
            }
            decimal discountAmount = (order.SubTotal * discountPercent) / 100;

            order.Discount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
            return order.Discount;
        }

        public async Task<decimal> CalculateShippingCostAsync(Order order, CancellationToken ct)
        {
            if (_failShippingForOrderIds != null && _failShippingForOrderIds.Contains(order.Id))
            {
                await Task.Delay(100, ct);
                throw new Exceptions.ShippingProviderUnavailableException("Shipping provider unavailable");
            }

            if (order.SubTotal >= 1000)
            {
                order.ShippingCost = 0.00m;
                return order.ShippingCost;
            }
            int totalQuantity = 0;
            decimal totalWeight = 0;
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
                if (product != null)
                {
                    totalQuantity += item.Quantity;
                    totalWeight += item.Quantity * product.WeightKg;
                }
            }
            decimal shippingCost = 5.00m + (1.50m * totalQuantity) + (2.00m * totalWeight);
            order.ShippingCost = Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);
            return order.ShippingCost;
        }
    }
}
