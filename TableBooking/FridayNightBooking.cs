using System;
using System.Collections.Generic;
using System.Text;

namespace TableBooking
{
    public sealed class FridayNightBooking
    {
        private readonly SemaphoreSlim _semaphore = new(1,1);

        public int TablesLeft { get; private set; } = 5;
        public int Booked { get; private set; }

        public async Task BookAsync(string customerName)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (TablesLeft <= 0)
                    return;

                await Task.Delay(20);

                TablesLeft--;
                Booked++;
                Console.WriteLine($"{customerName} booked a table. Left={TablesLeft}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        //    namespace TableBooking
        //{
        //    public class FridayNightBooking
        //    {
        //        private readonly object _lock = new();

        //        public int TablesLeft { get; private set; } = 20;   
        //        public int Booked { get; private set; } = 0;

        //        public void Book(string customerName)
        //        {
        //            lock (_lock)
        //            {
        //                if (TablesLeft <= 0)
        //                {
        //                    Console.WriteLine($"{customerName} tried to book, but no tables left!");
        //                    return;
        //                }

        //                Thread.Sleep(20);

        //                TablesLeft--;
        //                Booked++;

        //                Console.WriteLine($"{customerName} booked a table. Left = {TablesLeft}");
        //            }
        //        }
        //    }
        //}







        //public void Book(string customerName)
        //{
        //    if (TablesLeft <= 0)
        //        return;

        //    Thread.Sleep(20);

        //    TablesLeft--;
        //    Booked++;
        //    Console.WriteLine($"{customerName} booked a table. Left={TablesLeft}");
        //}
    }

}
