using System.ComponentModel.DataAnnotations;

namespace ShopInstallment.Models
{
    public class EscrowAccount
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SellerId { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0m;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
