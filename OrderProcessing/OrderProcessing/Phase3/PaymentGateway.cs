using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase3
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken cancellationToken = default);
        Task<bool> RefundAsync(string transactionId, CancellationToken cancellationToken = default);
    }

    public class PaymentGateway : IPaymentGateway
    {
        public async Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken cancellationToken = default)
        {
            await Task.Delay(150, cancellationToken);

            if (amount <= 0)
            {
                return new PaymentResult
                {
                    Success = false,
                    Status = PaymentStatus.Declined,
                    Message = "Invalid order amount."
                };
            }

            return new PaymentResult
            {
                Success = true,
                TransactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Status = PaymentStatus.Captured,
                Message = "Payment processed successfully."
            };
        }

        public async Task<bool> RefundAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);
            return true;
        }
    }
}
