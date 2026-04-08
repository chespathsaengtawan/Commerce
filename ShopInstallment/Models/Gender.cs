using System.ComponentModel.DataAnnotations;

namespace ShopInstallment.Models;
public class Gender
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Inactive
}