using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Shared
{
    public static class DataFiles
    {
        public static readonly string DirectoryPath = Path.Combine(AppContext.BaseDirectory, "Data");
        public const string Customers = "customers.json";
        public const string Products = "products.json";
        public const string Coupons = "coupons.json";
        public const string IncomingOrders = "incoming_orders.json";
        public const string Orders = "orders.json";
        public const string Payments = "payments.json";
        public const string Emails = "emails.json";
        public const string EmailRetry = "email_retry.json";
    }
}
