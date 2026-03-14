using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class InstallmentPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        public int InstallmentMonths { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyPayment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Completed, Defaulted

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        public virtual ICollection<InstallmentPayment> InstallmentPayments { get; set; } = new List<InstallmentPayment>();
    }
}
