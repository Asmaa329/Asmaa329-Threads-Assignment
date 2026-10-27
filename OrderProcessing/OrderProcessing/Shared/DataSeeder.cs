using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Shared
{
    public class DataSeeder
    {
        private readonly JsonFileStore _fileStore;

        public DataSeeder(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public async Task SeedAsync(CancellationToken ct)
        {
            var customers = new List<Customer>
            {
                CreateCustomer(1, "Ahmed Ali", true, LoyaltyTier.Gold),
                CreateCustomer(2, "Sara Mohamed", true, LoyaltyTier.Silver),
                CreateCustomer(3, "Omar Hassan", true, LoyaltyTier.Bronze),
                CreateCustomer(4, "Mona Ibrahim", false, LoyaltyTier.Silver),
                CreateCustomer(5, "Youssef Adel", true, LoyaltyTier.Gold),
                CreateCustomer(6, "Nour Khaled", true, LoyaltyTier.Bronze),
                CreateCustomer(7, "Karim Samir", true, LoyaltyTier.Silver),
                CreateCustomer(8, "Laila Mahmoud", true, LoyaltyTier.Gold),
                CreateCustomer(9, "Hassan Tarek", true, LoyaltyTier.Bronze),
                CreateCustomer(10, "Dina Fathy", true, LoyaltyTier.Silver)
            };

            await _fileStore.WriteAsync("customers.json", customers, ct);
            Console.WriteLine($"[Seeder] customers.json > {customers.Count} records"); //[cite: 13]

            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 1200.00m, StockQuantity = 10, WeightKg = 2.2m },
                new Product { Id = 2, Name = "Smartphone", Price = 800.00m, StockQuantity = 15, WeightKg = 0.4m },
                new Product { Id = 3, Name = "Wireless Mouse", Price = 25.00m, StockQuantity = 2, WeightKg = 0.1m },
                new Product { Id = 4, Name = "Mechanical Keyboard", Price = 90.00m, StockQuantity = 1, WeightKg = 0.9m },
                new Product { Id = 5, Name = "USB-C Hub", Price = 45.00m, StockQuantity = 30, WeightKg = 0.2m },
                new Product { Id = 6, Name = "Headphones", Price = 150.00m, StockQuantity = 12, WeightKg = 0.3m },
                new Product { Id = 7, Name = "27\" Monitor", Price = 350.00m, StockQuantity = 6, WeightKg = 5.0m },
                new Product { Id = 8, Name = "Webcam", Price = 70.00m, StockQuantity = 20, WeightKg = 0.2m }
            };
            await _fileStore.WriteAsync("products.json", products, ct);
            Console.WriteLine($"[Seeder] products.json > {products.Count} records"); //[cite: 13]

            var coupons = new List<Coupon>
            {
                new Coupon { Code = "SAVE10", DiscountPercent = 10, ExpiryDate = DateTime.Today.AddDays(180) },
                new Coupon { Code = "WELCOME15", DiscountPercent = 15, ExpiryDate = DateTime.Today.AddDays(90) },
                new Coupon { Code = "FLASH20", DiscountPercent = 20, ExpiryDate = DateTime.Today.AddDays(7) },
                new Coupon { Code = "OLD20", DiscountPercent = 20, ExpiryDate = DateTime.Today.AddDays(-30) }
            };
            await _fileStore.WriteAsync("coupons.json", coupons, ct);
            Console.WriteLine($"[Seeder] coupons.json > {coupons.Count} records"); //[cite: 13]

            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 1, CouponCode = "SAVE10", Items = { new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 0 }, new OrderItem { ProductId = 3, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 2, CustomerId = 2, CouponCode = null, Items = { new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 3, CustomerId = 3, CouponCode = "WELCOME15", Items = { new OrderItem { ProductId = 5, Quantity = 2, UnitPrice = 0 }, new OrderItem { ProductId = 8, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 4, CustomerId = 4, CouponCode = null, Items = { new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 5, CustomerId = 5, CouponCode = null, Items = { new OrderItem { ProductId = 7, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 6, CustomerId = 6, CouponCode = "FLASH20", Items = { new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 7, CustomerId = 7, CouponCode = null, Items = { new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0 }, new OrderItem { ProductId = 8, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 8, CustomerId = 8, CouponCode = "SAVE10", Items = { new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 9, CustomerId = 9, CouponCode = null,Items = { new OrderItem { ProductId = 3, Quantity = 5, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 10, CustomerId = 10, CouponCode = null, Items = { new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 11, CustomerId = 1, CouponCode = null, Items = { new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 12, CustomerId = 2, CouponCode = "OLD20", Items = { new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 13, CustomerId = 3, CouponCode = null, Items = { new OrderItem { ProductId = 6, Quantity = 2, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 14, CustomerId = 5, CouponCode = "WELCOME15", Items = { new OrderItem { ProductId = 8, Quantity = 2, UnitPrice = 0 }, new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 15, CustomerId = 99, CouponCode = null, Items = { new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 16, CustomerId = 6, CouponCode = null, Items = { new OrderItem { ProductId = 7, Quantity = 1, UnitPrice = 0 }, new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 17, CustomerId = 7, CouponCode = "SAVE10", Items = { new OrderItem { ProductId = 2, Quantity = 1,UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 18, CustomerId = 8, CouponCode = "FAKE50", Items = { new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 19, CustomerId = 9, CouponCode = null, Items = { new OrderItem { ProductId = 8, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now },
                new Order { Id = 20, CustomerId = 10, CouponCode = "FLASH20", Items = { new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 0 }, new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0 } }, Status = OrderStatus.Pending, CreatedAt = DateTime.Now }
            };
            await _fileStore.WriteAsync("incoming_orders.json", orders, ct);
            Console.WriteLine($"[Seeder] incoming_orders.json > {orders.Count} records"); 
        }

        private Customer CreateCustomer(int id, string name, bool isActive, LoyaltyTier loyaltyTier)
        {
            var parts = name.ToLower().Split(' ');
            string email = $"{parts[0]}.{parts[1]}@example.com";

            return new Customer
            {
                Id = id,
                Name = name,
                Email = email,
                IsActive = isActive,
                LoyaltyTier = loyaltyTier
            };
        }
    }
}

