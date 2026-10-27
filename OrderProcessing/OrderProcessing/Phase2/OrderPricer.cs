using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase2
{
    public class OrderPricer
    {
        private readonly PricingService _pricingService;
        public OrderPricer(PricingService pricingService)
        {
            _pricingService = pricingService;
        }
        public async Task PriceOrderAsync(Order order, CancellationToken ct)
        {
            await _pricingService.CalculateSubTotalAsync(order, ct);

            var discountTask = _pricingService.GetDiscountAsync(order, ct);
            var shippingTask = _pricingService.CalculateShippingCostAsync(order, ct);

            await Task.WhenAll(discountTask, shippingTask);

            decimal total = (order.SubTotal - order.Discount) + order.ShippingCost;
            order.TotalAmount = Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }
        public async Task PriceOrderSequentialAsync(Order order, CancellationToken ct)
        {
            await _pricingService.CalculateSubTotalAsync(order, ct);
            await _pricingService.GetDiscountAsync(order, ct);
            await _pricingService.CalculateShippingCostAsync(order, ct);

            decimal total = (order.SubTotal - order.Discount) + order.ShippingCost;
            order.TotalAmount = Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }


        

    }
}
