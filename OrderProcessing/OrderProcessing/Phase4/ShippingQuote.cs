using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase4
{
    public record ShippingQuote(string Carrier, decimal Price, long ElapsedMs);

    public interface IShippingCarrier
    {
        string Name { get; }
        Task<decimal> GetQuoteAsync(Order order, CancellationToken ct);
    }

    public class ShippingQuoteService
    {
        private readonly IEnumerable<IShippingCarrier> _carriers;

        public ShippingQuoteService(IEnumerable<IShippingCarrier> carriers)
        {
            _carriers = carriers;
        }

        public async Task<ShippingQuote> GetFastestQuoteAsync(Order order, decimal basePrice, CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(1200); 

            var activeTasks = _carriers.Select(c => RunCarrierAsync(c, order, basePrice, cts.Token)).ToList();

            while (activeTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(activeTasks);
                activeTasks.Remove(finishedTask);

                try
                {
                    var quote = await finishedTask;
                    cts.Cancel(); 
                    return quote;
                }
                catch
                {
                }
            }

            return new ShippingQuote("Flat rate", 25.00m, 1200);
        }

        private async Task<ShippingQuote> RunCarrierAsync(IShippingCarrier carrier, Order order, decimal basePrice, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            decimal price = await carrier.GetQuoteAsync(order, ct);
            sw.Stop();

            return new ShippingQuote(carrier.Name, price, sw.ElapsedMilliseconds);
        }
    }
}
