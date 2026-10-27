using OrderProcessing.OrderProcessing.Phase1;
using OrderProcessing.OrderProcessing.Phase3;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase2
{
    public class Phase2Runner
    {
        private readonly PricingService _pricingService;
        private readonly OrderPricer _orderPricer;
        private readonly JsonFileStore _fileStore;
        private readonly OrderRepository _orderRepository; 

        public Phase2Runner(
            JsonFileStore fileStore,
            OrderRepository orderRepository,
            ProductRepository productRepository,
            CustomerRepository customerRepository,
            CouponRepository couponRepository)
        {
            _fileStore = fileStore;
            var failShippingIds = new[] { 7 };
            _orderRepository = orderRepository; 
        _pricingService = new PricingService(
                orderRepository,
                productRepository,
                customerRepository,
                couponRepository,
                failShippingIds
            );

            _orderPricer = new OrderPricer(_pricingService);
        }

        public async Task RunAsync(Order sampleOrder, CancellationToken ct)
        {
            Console.WriteLine("=== Starting Phase 2 Benchmarks ===");

            var swSequential = Stopwatch.StartNew();
            await _orderPricer.PriceOrderSequentialAsync(sampleOrder, ct);
            swSequential.Stop();
            Console.WriteLine($"Sequential Execution Time: {swSequential.ElapsedMilliseconds} ms");

            var swConcurrent = Stopwatch.StartNew();
            await _orderPricer.PriceOrderAsync(sampleOrder, ct);
            swConcurrent.Stop();
            Console.WriteLine($"Concurrent Execution Time: {swConcurrent.ElapsedMilliseconds} ms");

            await RunHeavyInvoiceDemo(sampleOrder, ct);
            Console.WriteLine("\n--- Processing All Validated Orders Concurrently ---");
            var incomingRepo = new IncomingOrderRepository(_fileStore);
            var incomingOrders = await incomingRepo.GetAllAsync(ct);

            foreach (var order in incomingOrders)
            {
                try
                {
                    await _orderPricer.PriceOrderAsync(order, ct);
                    order.Status = OrderStatus.Validated;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Handling Error] Order #{order.Id}: {ex.Message}");
                    order.Status = OrderStatus.Failed;
                    order.FailureReason = ex.Message;
                }

                await _orderRepository.SaveAsync(order, ct);
            }

            Console.WriteLine(">>> Finished saving all orders to orders.json successfully!");
        
        }

        private async Task RunHeavyInvoiceDemo(Order order, CancellationToken ct)
        {
            Console.WriteLine("\n--- Running Heavy Invoice Generation (Task 8.5) ---");
            var invoiceGen = new InvoiceGenerator();
            await invoiceGen.GenerateAsync(order, ct);
        }
    }

}
    
    

