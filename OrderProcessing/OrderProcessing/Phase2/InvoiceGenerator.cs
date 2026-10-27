using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase2
{
    public class InvoiceGenerator
    {
        public async Task GenerateAsync(Order order , CancellationToken ct)
        {
            await Task.Run(() =>
            {
                long checksum = 0;
                for (int i = 0; i < 20_000_000; i++)
                {
                    checksum += i ^ order.Id;
                }

                string invoiceContent = $"Invoice for Order #{order.Id} - Checksum: {checksum}";
                System.IO.File.WriteAllText($"invoice_{order.Id}.txt", invoiceContent);
            });

            Console.WriteLine($"Invoice generated for Order #{order.Id} in background.");
        }

    }
}
