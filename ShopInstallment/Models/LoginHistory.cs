using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(50)]
        public string? LoginProvider { get; set; } // Local, Google, Facebook

        public bool IsSuccessful { get; set; } = true;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
