namespace TableBooking
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var booking = new FridayNightBooking();
            var tasks = new List<Task>();

            for (int i = 0; i < 40; i++)
            {
                string name = $"Customer-{i + 1:00}";
                tasks.Add(booking.BookAsync(name));
            }
            await Task.WhenAll(tasks);
            Console.WriteLine($"Booked={booking.Booked}, TablesLeft={booking.TablesLeft}");


        }
    }
}
