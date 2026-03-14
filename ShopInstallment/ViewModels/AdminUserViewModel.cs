using System.ComponentModel.DataAnnotations;
using ShopInstallment.Models;

namespace ShopInstallment.ViewModels
{
    public class AdminUserRow
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsLockedOut { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();

        public bool IsAdmin => Roles.Contains("Admin");
        public bool IsSeller => Roles.Contains("Seller");
        public string DisplayName => string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public class AdminUserListViewModel
    {
        public List<AdminUserRow> Users { get; set; } = new();
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int SellerUsers { get; set; }
        public int CustomerUsers { get; set; }
    }
}
