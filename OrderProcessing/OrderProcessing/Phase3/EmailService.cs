using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase3
{

    public interface IEmailService
    {
        Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct);
        Task SendConfirmationWithRetryAsync(Order order, Customer customer, CancellationToken ct);
    }

    public class EmailService : IEmailService
    {
        private readonly JsonFileStore _store = new();
        private readonly Dictionary<int, int> _failAttempts;

        public EmailService(Dictionary<int, int> failAttempts)
        {
            _failAttempts = failAttempts;
        }

        public async Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct)
        {
            await Task.Delay(200, ct);
            await Task.Delay(300, ct);
            await Task.Delay(500, ct);

            if (_failAttempts.TryGetValue(order.Id, out int failCount) && failCount > 0)
            {
                _failAttempts[order.Id]--;
                throw new Exception("SmtpUnavailableException: SMTP server unavailable");
            }

            var emailMsg = new EmailMessage
            {
                OrderId = order.Id,
                To = customer.Email,
                Subject = $"Order #{order.Id} Confirmed",
                Body = $"Dear {customer.Name}, your order has been successfully completed.",
                SentAt = DateTime.Now,
                Attempts = 1
            };

            var emails = await _store.ReadAsync<List<EmailMessage>>(DataFiles.Emails, ct);
            emails.Add(emailMsg);
            await _store.WriteAsync(DataFiles.Emails, emails, ct);
        }

        public async Task SendConfirmationWithRetryAsync(Order order, Customer customer, CancellationToken ct)
        {
            int[] retryDelays = { 500, 1000, 2000 };
            int maxAttempts = 4;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await Task.Delay(200, ct);
                    await Task.Delay(300, ct);
                    await Task.Delay(500, ct);

                    if (_failAttempts.TryGetValue(order.Id, out int remainingFails) && remainingFails > 0)
                    {
                        if (remainingFails < int.MaxValue)
                        {
                            _failAttempts[order.Id]--;
                        }
                        throw new Exception("SMTP unavailable");
                    }

                    var emailMsg = new EmailMessage
                    {
                        OrderId = order.Id,
                        To = customer.Email,
                        Subject = $"Order #{order.Id} Confirmed",
                        Body = $"Dear {customer.Name}, your order has been successfully completed.",
                        SentAt = DateTime.Now,
                        Attempts = attempt
                    };

                    var emails = await _store.ReadAsync<List<EmailMessage>>(DataFiles.Emails, ct);
                    emails.Add(emailMsg);
                    await _store.WriteAsync(DataFiles.Emails, emails, ct);
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        var failedEmail = new EmailMessage
                        {
                            OrderId = order.Id,
                            To = customer.Email,
                            Subject = $"Order #{order.Id} Confirmation Failed",
                            Body = "Failed to send confirmation.",
                            Attempts = maxAttempts,
                            LastError = ex.Message
                        };

                        var retries = await _store.ReadAsync<List<EmailMessage>>(DataFiles.EmailRetry, ct);
                        retries.Add(failedEmail);
                        await _store.WriteAsync(DataFiles.EmailRetry, retries, ct);
                        Console.WriteLine($"[Email] Email for Order #{order.Id} queued for retry later.");
                        return;
                    }

                    await Task.Delay(retryDelays[attempt - 1], ct);
                }
            }
        }
    }

}
