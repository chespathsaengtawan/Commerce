using ShopInstallment.Models;

namespace ShopInstallment.Services.Payments
{
    public interface IPaymentProvider
    {
        Task<string> CreateChargeAsync(Order order, decimal amount, PaymentMethod method, IDictionary<string, string>? metadata = null);
        Task<bool> CaptureAsync(string transactionId);
        Task<bool> RefundAsync(string transactionId, decimal amount, string reason);
        Task<PaymentStatusResult> GetStatusAsync(string transactionId);
    }

    public class PaymentStatusResult
    {
        public string Status { get; set; } = "Pending";
        public string? FailureMessage { get; set; }
        public string? ReceiptUrl { get; set; }
    }
}
