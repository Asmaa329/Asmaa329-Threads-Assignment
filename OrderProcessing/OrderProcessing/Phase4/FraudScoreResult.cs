using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase4
{
    public class FraudScoreResult
    {
        public int OrderId { get; set; }
        public int Score { get; set; }
    }

    public class FraudScorer
    {
        public static (int FlaggedCount, List<int> FirstFiveIds, long ElapsedMs) ScoreOrdersParallel()
        {
            var flaggedIds = new ConcurrentBag<int>();
            var sw = Stopwatch.StartNew();

            Parallel.For(1, 10001, i =>
            {
                if (CalculateScore(i) >= 90)
                {
                    flaggedIds.Add(i);
                }
            });

            sw.Stop();
            var sortedIds = flaggedIds.OrderBy(id => id).ToList();
            return (sortedIds.Count, sortedIds.Take(5).ToList(), sw.ElapsedMilliseconds);
        }

        private static int CalculateScore(int orderId)
        {
            long h = orderId;
            for (int i = 0; i < 100_000; i++)
            {
                h = (h * 31 + 1) % 1_000_000_007;
            }
            return (int)(h % 100);
        }
    }
}
