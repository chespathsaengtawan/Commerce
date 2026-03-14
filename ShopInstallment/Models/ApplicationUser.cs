using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ShopInstallment.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? CustomerCode { get; set; }

        [StringLength(20)]
        public string? PhoneNumber2 { get; set; }

        [StringLength(50)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(100)]
        public string? BankAccountName { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    }
}
