using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;
using System.Security.Claims;

namespace ShopInstallment.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrdersController(ApplicationDbContext context) => _context = context;

        [HttpGet("/Seller/Orders")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSellerRoute = HttpContext.Request.Path.StartsWithSegments("/Seller/Orders", StringComparison.OrdinalIgnoreCase);

            if (isSellerRoute && !User.IsInRole("Seller"))
            {
                return Forbid();
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminIndex));
            }

            if (User.IsInRole("Seller"))
            {
                var sellerOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Seller)
                    .Include(o => o.Payments)
                    .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == userId))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return View(sellerOrders);
            }

            var items = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        [HttpGet("/Seller/Orders/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSellerRoute = HttpContext.Request.Path.StartsWithSegments("/Seller/Orders", StringComparison.OrdinalIgnoreCase);

            if (isSellerRoute && !User.IsInRole("Seller"))
            {
                return Forbid();
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminDetails), new { id });
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Seller)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (User.IsInRole("Seller"))
            {
                var ownsAnyItems = order.OrderItems.Any(oi => oi.Product?.SellerId == userId);
                if (!ownsAnyItems) return Forbid();
            }
            else if (!string.Equals(order.CustomerId, userId, StringComparison.Ordinal))
            {
                return Forbid();
            }

            // Load seller settings (simplified: using customer as seller placeholder)
            var sellerSetting = await _context.SellerPaymentSettings.FirstOrDefaultAsync(s => s.SellerId == order.CustomerId);
            sellerSetting ??= new SellerPaymentSetting { SellerId = order.CustomerId };
            ViewBag.SellerSetting = sellerSetting;

            // Load user's Coin wallet
            var wallet = await _context.CoinWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            ViewBag.CoinBalance = wallet?.Balance ?? 0m;

            return View(order);
        }

        // Admin Methods
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Seller)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            return View("~/Views/Admin/Orders/Index.cshtml", orders);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Seller)
                .Include(o => o.Payments)
                .Include(o => o.InstallmentPlan)
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null) return NotFound();
            
            return View("~/Views/Admin/Orders/AdminDetails.cshtml", order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminConfirmOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Confirmed";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminDetails), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminDetails), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCompleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Completed";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminDetails), new { id });
        }
    }
}
