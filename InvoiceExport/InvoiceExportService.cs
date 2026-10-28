using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace InvoiceExport
{
    public sealed class InvoiceExportService
    {
        readonly OrderDatabase _db;
        readonly CustomerFileStore _customers;
        readonly InvoiceFileWriter _invoices;

        public InvoiceExportService(OrderDatabase db, CustomerFileStore customers, InvoiceFileWriter invoices)
        {
            _db = db;
            _customers = customers;
            _invoices = invoices;
        }


        public async Task ExportAllPendingAsync()
        {
            var watch = Stopwatch.StartNew();

            List<Order> orders = await _db.GetPendingOrdersAsync();

            var exportTasks = new List<Task>();

            foreach (Order order in orders)
            {
                Task task = Task.Run(async () =>
                {
                    CustomerProfile customer = await _customers.ReadProfileAsync(order.CustomerId);
                    string path = await _invoices.WriteInvoiceAsync(order, customer);

                    lock (Console.Out)
                    {
                        Console.WriteLine($"Wrote {path}");
                    }
                });

                exportTasks.Add(task);
            }
            await Task.WhenAll(exportTasks);

            Console.WriteLine($"Export finished in {watch.Elapsed.TotalSeconds:F1} s");
        }

    }

}
