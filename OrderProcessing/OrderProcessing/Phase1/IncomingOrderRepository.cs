using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class IncomingOrderRepository
    {
        private readonly JsonFileStore _fileStore;

        public IncomingOrderRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        public async Task<List<Order>> GetAllAsync(CancellationToken ct)
        {
            Log.Info("IncomingOrderRepository => Open connection");
            await Task.Delay(200,ct);
            Log.Info("IncomingOrderRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            return await _fileStore.ReadAsync<List<Order>>("incoming_orders.json",ct);

        }
    }
}
