using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class Phase1Runner
    {
        public static async Task RunAsync(CancellationToken ct)
        {
            Console.WriteLine("=== Running Phase 1 Runner ===");

            var fileStore = new JsonFileStore();
            var seeder = new DataSeeder(fileStore);
            await seeder.SeedAsync(ct);

            var customerRepo = new CustomerRepository(fileStore);
            var productRepo = new ProductRepository(fileStore);
            var couponRepo = new CouponRepository(fileStore);
            var orderRepo = new OrderRepository(fileStore);
            var incomingRepo = new IncomingOrderRepository(fileStore);

            var validator = new OrderValidator(customerRepo, productRepo, couponRepo, orderRepo);

            var incomingOrders = await incomingRepo.GetAllAsync(ct);

            foreach (var order in incomingOrders)
            {
                await validator.ValidateOrderAsync(order, ct);
            }

            var validatedOrders = await orderRepo.GetByStatusAsync(OrderStatus.Validated, ct);
            var rejectedOrders = await orderRepo.GetByStatusAsync(OrderStatus.Rejected, ct);

            Console.WriteLine("===== Validation Summary =====");
            Console.WriteLine($"Total Orders: {incomingOrders.Count}");
            Console.WriteLine($"Validated: {validatedOrders.Count}");
            Console.WriteLine($"Rejected: {rejectedOrders.Count}");

            foreach (var rej in rejectedOrders)
            {
                Console.WriteLine($"Order #{rej.Id}: {rej.FailureReason}");
            }
        }
    }
}
