using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class OrderRepository
    {
        private readonly JsonFileStore _fileStore;
        public OrderRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
       public async Task SaveAsync(Order order, CancellationToken ct)
{
          Log.Info("OrderRepository => Open connection");
          await Task.Delay(200, ct);
          Log.Info("OrderRepository => Execute query");
          await Task.Delay(Random.Shared.Next(300, 601), ct);
    
    // 1. قراءة اللستة الحالية
           var orderList = await _fileStore.ReadAsync<List<Order>>("orders.json", ct);
    
    // 2. البحث هل الأوردر موجود قبل كده؟
           var existingOrderIndex = orderList.FindIndex(o => o.Id == order.Id);
    
           if (existingOrderIndex != -1)
           {
        // لو موجود، حدث بياناته بالقيم الجديدة (SubTotal, TotalAmount, Status, etc.)
             orderList[existingOrderIndex] = order;
           }
           else
           {
        // لو مش موجود، ضيفه جديد
            orderList.Add(order);
           }
    
    // 3. حفظ اللستة النهائية بنظافة في الملف
          await _fileStore.WriteAsync<List<Order>>("orders.json", orderList, ct);
}
        public async Task<List<Order>> GetAllAsync(CancellationToken ct)
        {
            Log.Info("OrderRepository => Open connection");
            await Task.Delay(200, ct);
            Log.Info("OrderRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            return await _fileStore.ReadAsync<List<Order>>("orders.json", ct);

        }
        public async Task<List<Order>>GetByStatusAsync(OrderStatus s, CancellationToken ct)
        {
            Log.Info("OrderRepository => Open connection");
            await Task.Delay(200, ct);
            Log.Info("OrderRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            var Orderlist = await _fileStore.ReadAsync<List<Order>>("orders.json",ct);
            return Orderlist.Where(c => c.Status == s).ToList();
        }

    }
}
