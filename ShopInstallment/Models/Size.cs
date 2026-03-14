using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInstallment.Models
{
    public class Size
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // e.g., S, M, L, XL

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Inactive

        // Foreign Key to Category
        public int? CategoryId { get; set; }

        // Navigation property
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}
