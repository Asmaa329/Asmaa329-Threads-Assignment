using OrderProcessing.OrderProcessing.Phase1;
using OrderProcessing.OrderProcessing.Phase2;
using OrderProcessing.OrderProcessing.Phase3;
using OrderProcessing.OrderProcessing.Shared;

namespace OrderProcessing.OrderProcessing
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            try
            {
                Console.WriteLine("=== Starting .NET Mini Project Order Processing System ===");

                var fileStore = new JsonFileStore();

                Console.WriteLine("\n--- Running Phase 1 ---");
                await Phase1Runner.RunAsync(ct);

                Console.WriteLine("\n--- Running Phase 2 ---");
                var orderRepo = new OrderRepository(fileStore);
                var productRepo = new ProductRepository(fileStore);
                var customerRepo = new CustomerRepository(fileStore);
                var couponRepo = new CouponRepository(fileStore);

                var phase2Runner = new Phase2Runner(fileStore, orderRepo, productRepo, customerRepo, couponRepo);

                var allOrders = await orderRepo.GetAllAsync(ct);
                var sampleOrder = allOrders.FirstOrDefault(o => o.Id == 1) ?? allOrders.First();

                await phase2Runner.RunAsync(sampleOrder, ct);

                var reportGen = new ReportGenerator(fileStore);
                Console.WriteLine("Generating report...");
                var report = await reportGen.GenerateAsync(ct);
                Console.WriteLine($"Report Generated Successfully! Total Revenue: {report.TotalRevenue}, Completed Orders: {report.CompletedCount}");

                Console.WriteLine("\n--- Running Phase 3 ---");
                var customerService = new CustomerService(fileStore);
                var inventoryService = new InventoryService(fileStore);

                var failShipping = new int[] { }; 
                var pricingService = new PricingService(orderRepo, productRepo, customerRepo, couponRepo, failShipping);
                var orderPricer = new OrderPricer(pricingService);
                IPaymentGateway paymentGateway = new PaymentGateway();
                var failAttempts = new Dictionary<int, int>(); 
                IEmailService emailService = new EmailService(failAttempts);
                var failSaveIds = new int[] { 13 }; 

                var orderProcessor = new OrderProcessor(
                    customerService,
                    inventoryService,
                    pricingService,
                    orderPricer,
                    paymentGateway,
                    emailService,
                    failSaveIds
                );

                foreach (var ord in allOrders.Take(5))
                {
                    await orderProcessor.ProcessOrderAsync(ord, ct);
                }
                Console.WriteLine("Phase 3 processing completed for sample orders.");

                Console.WriteLine("\n--- Running Phase 4 ---");

                int sampleCustomerId = 1; 

                var profileTask = customerRepo.GetByIdAsync(sampleCustomerId, ct);
                var ordersTask = orderRepo.GetAllAsync(ct); 
                var paymentsTask = fileStore.ReadAsync<List<PaymentTransaction>>(DataFiles.Payments, ct);
                var emailsTask = fileStore.ReadAsync<List<EmailMessage>>(DataFiles.Emails, ct);

                try
                {
                    await Task.WhenAll(profileTask, ordersTask, paymentsTask, emailsTask);

                    Console.WriteLine($"[Dashboard] Successfully loaded data for Customer #{sampleCustomerId}");
                    Console.WriteLine($"[Dashboard] Orders Count: {(await ordersTask).Count}");
                    Console.WriteLine($"[Dashboard] Payments Count: {(await paymentsTask).Count}");
                    Console.WriteLine($"[Dashboard] Emails Count: {(await emailsTask).Count}");
                }
                catch (Exception)
                {
                    var exceptions = new List<Exception>();
                    foreach (Task task in new Task[] { profileTask, ordersTask, paymentsTask, emailsTask })
                    {
                        if (task.IsFaulted && task.Exception != null)
                        {
                            exceptions.AddRange(task.Exception.InnerExceptions);
                        }
                    }
                    if (exceptions.Count > 0)
                    {
                        Console.WriteLine($"[Dashboard] Encounted {exceptions.Count} errors while loading dashboard data.");
                    }
                }

                Console.WriteLine("\n=== All Phases Executed Successfully! ===");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] An unhandled exception occurred: {ex.Message}");
                Console.ResetColor();
            }

           
            
        }
    }
}

