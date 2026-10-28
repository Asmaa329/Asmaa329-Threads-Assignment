using OrderProcessing.OrderProcessing.Phase1;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderProcessing.OrderProcessing.Phase2
{
    public class ReportGenerator
    {
        private readonly OrderRepository _orderRepo;
        private readonly ProductRepository _productRepo;

        public ReportGenerator(JsonFileStore fileStore)
        {
            _orderRepo = new OrderRepository(fileStore);
            _productRepo = new ProductRepository(fileStore);
        }

        public async Task<Report> GenerateAsync(CancellationToken ct)
        {
            var orders = await _orderRepo.GetAllAsync(ct);

            decimal totalRevenue = 0;
            int completedCount = 0;

            foreach (var o in orders)
            {
                if (o.Status == OrderStatus.Completed)
                {
                    totalRevenue += o.TotalAmount;
                    completedCount++;
                }
            }

            var productSales = new Dictionary<int, int>(); 

            foreach (var o in orders)
            {
                if (o.Status == OrderStatus.Completed)
                {
                    foreach (var item in o.Items)
                    {
                        
                        if (productSales.ContainsKey(item.ProductId))
                        {
                            productSales[item.ProductId] += item.Quantity;
                        }
                        else
                        {
                            productSales[item.ProductId] = item.Quantity;
                        }
                    }
                }
            }

            int topProductId = 0;
            int maxQty = 0;
            foreach (var pair in productSales)
            {
                if (pair.Value > maxQty)
                {
                    maxQty = pair.Value;
                    topProductId = pair.Key;
                }
            }

            string topProduct = "None";
            int topQty = 0;
            if (maxQty > 0)
            {
                var product = await _productRepo.GetByIdAsync(topProductId, ct);
                topProduct = product?.Name ?? "Unknown";
                topQty = maxQty;
            }

            int rejectedCount = 0;
            int failedCount = 0;
            int paymentFailedCount = 0;

            foreach (var o in orders)
            {
                if (o.Status == OrderStatus.Rejected) rejectedCount++;
                else if (o.Status == OrderStatus.Failed) failedCount++;
                else if (o.Status == OrderStatus.PaymentFailed) paymentFailedCount++;
            }

            var report = new Report
            {
                TotalOrders = orders.Count,
                TotalRevenue = totalRevenue,
                CompletedCount = completedCount,
                RejectedCount = rejectedCount,
                FailedCount = failedCount,
                PaymentFailedCount = paymentFailedCount,
                TopSellingProduct = topProduct,
                TopSellingQuantity = topQty,
                AverageOrderValue = completedCount > 0 ? totalRevenue / completedCount : 0m,
                GeneratedAt = DateTime.Now
            };

            string reportsDir = Path.Combine(DataFiles.DirectoryPath, "reports");
            Directory.CreateDirectory(reportsDir);

            string fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(reportsDir, fileName);

            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, options), ct);

            return report;
        }
    }
}