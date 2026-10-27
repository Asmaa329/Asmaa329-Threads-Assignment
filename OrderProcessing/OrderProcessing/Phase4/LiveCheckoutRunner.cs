using OrderProcessing.OrderProcessing.Phase3;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase4
{
    public class LiveCheckoutRunner
    {
        private readonly OrderProcessor _orderProcessor;

        public LiveCheckoutRunner(OrderProcessor orderProcessor)
        {
            _orderProcessor = orderProcessor;
        }

        public async Task RunAsync(List<Order> orders, CancellationToken ct)
        {
            var tasks = orders.Select(o => ProcessAndTrackAsync(o, ct)).ToHashSet();
            int completedCount = 0;
            int totalOrders = orders.Count;

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);
                completedCount++;

                var (order, elapsed) = await finishedTask;
                string reasonText = !string.IsNullOrEmpty(order.FailureReason) ? $" ({order.FailureReason})" : "";
                Console.WriteLine($"[{completedCount}/{totalOrders}] Order #{order.Id} -> {order.Status}{reasonText} after {elapsed.TotalSeconds:F1} s");
            }
        }

        private async Task<(Order, TimeSpan)> ProcessAndTrackAsync(Order order, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            await _orderProcessor.ProcessOrderAsync(order, ct);
            sw.Stop();
            return (order, sw.Elapsed);
        }
    }
}
