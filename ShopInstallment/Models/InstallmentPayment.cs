using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class InstallmentPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InstallmentPlanId { get; set; }

        public int PaymentNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Late, Missed

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation property
        [ForeignKey("InstallmentPlanId")]
        public virtual InstallmentPlan? InstallmentPlan { get; set; }
    }
}
