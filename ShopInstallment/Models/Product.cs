using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }

        // Optional daily rent price
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RentPricePerDay { get; set; }

        public int Stock { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Active"; // Active, Inactive, OutOfStock

        [StringLength(500)]
        public string? MainImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        [Required]
        public string SellerId { get; set; } = string.Empty;

        // Taxonomy
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }

        // Navigation properties
        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual Color? Color { get; set; }
        public virtual Size? Size { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductRating> Ratings { get; set; } = new List<ProductRating>();
    }
}
