using ShopInstallment.Models;

namespace ShopInstallment.ViewModels
{
    public class AdminDashboardViewModel
    {
        // User stats
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int SellerUsers { get; set; }
        public int CustomerUsers { get; set; }

        // Orders
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalRemainingAmount { get; set; }

        // Products
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }

        // Payments
        public int TotalPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int EscrowHoldPayments { get; set; }
        public int EscrowFrozenPayments { get; set; }
        public decimal TotalPaymentAmount { get; set; }

        // Wallets
        public decimal TotalCoinBalance { get; set; }

        // Recent lists
        public List<Order> RecentOrders { get; set; } = new();
        public List<Payment> RecentPayments { get; set; } = new();
    }
}
