using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string VariantName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAdjustment { get; set; }

        public int Stock { get; set; }

        [StringLength(100)]
        public string? SKU { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
