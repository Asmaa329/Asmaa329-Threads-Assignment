using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class ProductRepository
    {
        private readonly JsonFileStore _fileStore;
        public ProductRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        {
            Log.Info("ProductRepository => Open connection");
            await Task.Delay(200,ct);
            Log.Info("ProductRepository => Execute query");
            var productlist = await _fileStore.ReadAsync<List<Product>>("products.json",ct);
            return productlist.FirstOrDefault(c => c.Id == id);
        }
        public async Task<bool> ReserveStockAsync(int id, int qty, CancellationToken ct)
        {
            Log.Info("ProductRepository => Open connection");
            await Task.Delay(200, ct);
            Log.Info("ProductRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);  

            var productlist = await _fileStore.ReadAsync<List<Product>>("products.json",ct);

            if (productlist == null)
                return false;
            var product = productlist.FirstOrDefault(p => p.Id == id);
            if (product == null || product.StockQuantity < qty)
            {
                return false;
            }
            product.StockQuantity -= qty;
            await _fileStore.WriteAsync("products.json", productlist,ct);

            return true;
        }
    }
}
