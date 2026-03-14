using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;
using ShopInstallment.ViewModels;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("/Admin")]
        public async Task<IActionResult> Index()
        {
            // User counts
            var totalUsers = await _userManager.Users.CountAsync();
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var sellers = await _userManager.GetUsersInRoleAsync("Seller");
            var adminCount = admins.Count;
            var sellerCount = sellers.Count;
            var customerCount = Math.Max(0, totalUsers - adminCount - sellerCount);

            // Orders summary
            var orders = _context.Orders.AsNoTracking();
            var totalOrders = await orders.CountAsync();
            var pendingOrders = await orders.CountAsync(o => o.Status == "Pending");
            var confirmedOrders = await orders.CountAsync(o => o.Status == "Confirmed");
            var completedOrders = await orders.CountAsync(o => o.Status == "Completed");
            var cancelledOrders = await orders.CountAsync(o => o.Status == "Cancelled");
            var totalOrderAmount = await orders.SumAsync(o => o.TotalAmount);
            var totalPaidAmount = await orders.SumAsync(o => o.PaidAmount);
            var totalRemainingAmount = await orders.SumAsync(o => o.RemainingAmount);

            // Products summary
            var products = _context.Products.AsNoTracking();
            var totalProducts = await products.CountAsync();
            var activeProducts = await products.CountAsync(p => p.Status == "Active");
            var inactiveProducts = totalProducts - activeProducts;

            // Payments summary
            var paymentsQuery = _context.Payments.AsNoTracking();
            var payments = await paymentsQuery.ToListAsync();
            var totalPayments = payments.Count;
            var completedPayments = payments.Count(p => p.Status == "Completed");
            var escrowHoldPayments = payments.Count(p => p.Status == "EscrowHold");
            var escrowFrozenPayments = payments.Count(p => p.Status == "EscrowFrozen");
            var totalPaymentAmount = payments.Sum(p => p.Amount);

            // Coin wallet summary
            var totalCoinBalance = await _context.CoinWallets.AsNoTracking().SumAsync(w => (decimal?)w.Balance) ?? 0m;

            // Recent orders and payments
            var recentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Seller)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentPayments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Order).ThenInclude(o => o.Customer)
                .Include(p => p.Order).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Seller)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                AdminUsers = adminCount,
                SellerUsers = sellerCount,
                CustomerUsers = customerCount,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalOrderAmount = totalOrderAmount,
                TotalPaidAmount = totalPaidAmount,
                TotalRemainingAmount = totalRemainingAmount,
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                TotalPayments = totalPayments,
                CompletedPayments = completedPayments,
                EscrowHoldPayments = escrowHoldPayments,
                EscrowFrozenPayments = escrowFrozenPayments,
                TotalPaymentAmount = totalPaymentAmount,
                TotalCoinBalance = totalCoinBalance,
                RecentOrders = recentOrders,
                RecentPayments = recentPayments
            };

            return View("~/Views/Admin/Index.cshtml", vm);
        }
    }
}
