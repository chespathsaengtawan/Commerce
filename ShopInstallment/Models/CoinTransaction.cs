using System.ComponentModel.DataAnnotations;

namespace ShopInstallment.Models
{
    public class CoinTransaction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = "TopUp"; // TopUp, Spend, Refund
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
