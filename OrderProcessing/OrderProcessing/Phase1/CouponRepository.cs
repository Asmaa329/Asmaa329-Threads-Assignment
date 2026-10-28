using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class CouponRepository
    {
        private readonly JsonFileStore _fileStore;

        public CouponRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        public async Task<Coupon?>GetByCodeAsync(string code, CancellationToken ct)
        {
            Log.Info("CouponRepository => Open connection");
            await Task.Delay(200, ct);
            Log.Info("CouponRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            var couponlist = await _fileStore.ReadAsync<List<Coupon>>("coupons.json",ct);
            return couponlist.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
