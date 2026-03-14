using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FeeAmount { get; set; } // Platform or method fee

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // PromptPay, BillPayment, TrueWallet, BankTransfer, Coin

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, EscrowHold, EscrowReleased, EscrowFrozen

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? ReceiptUrl { get; set; }

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
