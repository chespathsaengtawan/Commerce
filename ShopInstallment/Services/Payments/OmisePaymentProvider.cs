using Microsoft.Extensions.Configuration;
using ShopInstallment.Models;

namespace ShopInstallment.Services.Payments
{
    public class OmisePaymentProvider : IPaymentProvider
    {
        private readonly IConfiguration _config;
        public OmisePaymentProvider(IConfiguration config)
        {
            _config = config;
        }

        public Task<string> CreateChargeAsync(Order order, decimal amount, PaymentMethod method, IDictionary<string, string>? metadata = null)
        {
            // TODO: integrate Omise SDK / HTTP API
            // Return a fake transaction id for now
            var txId = $"omise_{Guid.NewGuid():N}";
            return Task.FromResult(txId);
        }

        public Task<bool> CaptureAsync(string transactionId)
        {
            // TODO: call capture API
            return Task.FromResult(true);
        }

        public Task<bool> RefundAsync(string transactionId, decimal amount, string reason)
        {
            // TODO: call refund API
            return Task.FromResult(true);
        }

        public Task<PaymentStatusResult> GetStatusAsync(string transactionId)
        {
            // TODO: query status API
            return Task.FromResult(new PaymentStatusResult { Status = "Completed" });
        }
    }
}
