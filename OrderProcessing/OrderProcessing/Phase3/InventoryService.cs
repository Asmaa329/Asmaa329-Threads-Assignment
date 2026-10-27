using OrderProcessing.OrderProcessing.Phase1;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase3
{
    public class InventoryService
    {
        private readonly ProductRepository _productRepo;

        public InventoryService(JsonFileStore fileStore)
        {
            _productRepo = new ProductRepository(fileStore);
        }
        public async Task<bool> ReserveStockAsync(Order order, CancellationToken ct)
        {
            var reservedItems = new List<(int ProductId, int Qty)>();

            foreach (var item in order.Items)
            {
                bool success = await _productRepo.ReserveStockAsync(item.ProductId, item.Quantity, ct);
                if (!success)
                {
                    await ReleaseStockAsync(new Order { Items = reservedItems.Select(r => new OrderItem { ProductId = r.ProductId, Quantity = r.Qty }).ToList() }, ct);
                    return false;
                }
                reservedItems.Add((item.ProductId, item.Quantity));
            }
            return true;
        }

        public async Task ReleaseStockAsync(Order order, CancellationToken ct)
        {
            foreach (var item in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId, ct);
                if (product != null)
                {
                    await _productRepo.ReserveStockAsync(item.ProductId, -item.Quantity, ct); 
                }
            }
        }
    }

}



