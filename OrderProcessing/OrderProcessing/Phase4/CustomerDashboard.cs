using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing
{
    public class CustomerDashboard
    {
        public Customer Customer { get; set; }
        public List<Order> Orders { get; set; }
        public List<PaymentTransaction> Payments { get; set; }
        public List<EmailMessage> Emails { get; set; }

        public CustomerDashboard(Customer customer, List<Order> orders, List<PaymentTransaction> payments, List<EmailMessage> emails)
        {
            Customer = customer;
            Orders = orders;
            Payments = payments;
            Emails = emails;
        }
    }

    public class CustomerDashboardService
    {
        private readonly HashSet<int> _failPaymentsForCustomerIds;

        public CustomerDashboardService(IEnumerable<int> failPaymentsForCustomerIds)
        {
            _failPaymentsForCustomerIds = new HashSet<int>(failPaymentsForCustomerIds);
        }

        public async Task<CustomerDashboard> LoadAsync(int customerId, CancellationToken ct)
        {
            var profileTask = GetProfileAsync(customerId, ct);
            var ordersTask = GetOrdersAsync(customerId, ct);
            var paymentsTask = GetPaymentsAsync(customerId, ct);
            var emailsTask = GetEmailsAsync(customerId, ct);

            try
            {
                await Task.WhenAll(profileTask, ordersTask, paymentsTask, emailsTask);
            }
            catch (Exception)
            {
                var exceptions = new List<Exception>();
                foreach (Task task in new Task[] { profileTask, ordersTask, paymentsTask, emailsTask })
                {
                    if (task.IsFaulted && task.Exception != null)
                    {
                        exceptions.AddRange(task.Exception.InnerExceptions);
                    }
                }
                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
                throw;
            }

            return new CustomerDashboard(
                profileTask.Result,
                ordersTask.Result,
                paymentsTask.Result,
                emailsTask.Result
            );
        }

        private async Task<Customer> GetProfileAsync(int id, CancellationToken ct)
        {
            await Task.Delay(200, ct);
            if (id == 99) throw new Exception($"Customer #{id} not found");
            return new Customer { Id = id, Name = "Omar Hassan", IsActive = true };
        }

        private async Task<List<Order>> GetOrdersAsync(int id, CancellationToken ct)
        {
            await Task.Delay(300, ct);
            return new List<Order>();
        }

        private async Task<List<PaymentTransaction>> GetPaymentsAsync(int id, CancellationToken ct)
        {
            await Task.Delay(250, ct);
            if (_failPaymentsForCustomerIds.Contains(id))
                throw new TimeoutException("Payments store timeout");
            return new List<PaymentTransaction>();
        }

        private async Task<List<EmailMessage>> GetEmailsAsync(int id, CancellationToken ct)
        {
            await Task.Delay(150, ct);
            return new List<EmailMessage>();
        }
    }
}