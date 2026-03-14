using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;
using System.Security.Claims;

namespace ShopInstallment.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Wallet
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await GetOrCreateWalletAsync(userId);

            // ดึงธุรกรรม 10 รายการล่าสุด
            var recentTransactions = await _context.CoinTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.RecentTransactions = recentTransactions;
            return View(wallet);
        }

        // GET: Wallet/TopUp
        public IActionResult TopUp()
        {
            return View();
        }

        // POST: Wallet/TopUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUp(decimal amount, string paymentMethod = "BankTransfer")
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "กรุณากรอกจำนวนเงินที่ถูกต้อง");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await GetOrCreateWalletAsync(userId);

            // เพิ่มยอด Coin
            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // บันทึกธุรกรรม
            var transaction = new CoinTransaction
            {
                UserId = userId,
                Amount = amount,
                Type = "TopUp",
                Reference = $"Top-up via {paymentMethod}",
                CreatedAt = DateTime.UtcNow
            };

            _context.CoinTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"เติมเงินสำเร็จ {amount:N2} Coin";
            return RedirectToAction(nameof(Index));
        }

        // POST: Wallet/Spend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Spend(decimal amount, string reference)
        {
            if (amount <= 0)
            {
                return Json(new { success = false, message = "จำนวนเงินไม่ถูกต้อง" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await GetOrCreateWalletAsync(userId);

            if (wallet.Balance < amount)
            {
                return Json(new { success = false, message = "ยอด Coin ไม่เพียงพอ" });
            }

            // หักยอด Coin
            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // บันทึกธุรกรรม
            var transaction = new CoinTransaction
            {
                UserId = userId,
                Amount = -amount,
                Type = "Spend",
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            };

            _context.CoinTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Json(new { success = true, balance = wallet.Balance });
        }

        // POST: Wallet/Refund
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(decimal amount, string reference)
        {
            if (amount <= 0)
            {
                return Json(new { success = false, message = "จำนวนเงินไม่ถูกต้อง" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await GetOrCreateWalletAsync(userId);

            // คืนยอด Coin
            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // บันทึกธุรกรรม
            var transaction = new CoinTransaction
            {
                UserId = userId,
                Amount = amount,
                Type = "Refund",
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            };

            _context.CoinTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Json(new { success = true, balance = wallet.Balance });
        }

        // GET: Wallet/History
        public async Task<IActionResult> History(int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transactions = await _context.CoinTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.CoinTransactions
                .Where(t => t.UserId == userId)
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(transactions);
        }

        // GET: /Seller/Wallet - Seller financial dashboard
        [HttpGet("/Seller/Wallet")]
        public async Task<IActionResult> SellerWallet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Seller"))
            {
                return Forbid();
            }

            // Seller's Coin wallet
            var wallet = await GetOrCreateWalletAsync(userId);

            // Seller's payment setting
            var sellerSetting = await _context.SellerPaymentSettings
                .FirstOrDefaultAsync(s => s.SellerId == userId);
            sellerSetting ??= new SellerPaymentSetting { SellerId = userId };

            // Orders with items sold by this seller
            var sellerOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == userId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Calculate seller-specific totals
            var totalOrderAmount = sellerOrders
                .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == userId))
                .Sum(oi => oi.TotalPrice);

            var totalPaidAmount = sellerOrders
                .SelectMany(o => o.Payments.Where(p => p.Status == "Completed"))
                .Sum(p => p.Amount);

            var completedOrders = sellerOrders.Count(o => o.Status == "Completed");
            var pendingOrders = sellerOrders.Count(o => o.Status == "Pending");
            var confirmedOrders = sellerOrders.Count(o => o.Status == "Confirmed");

            // Recent transactions
            var recentTransactions = await _context.CoinTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.Wallet = wallet;
            ViewBag.SellerSetting = sellerSetting;
            ViewBag.TotalOrderAmount = totalOrderAmount;
            ViewBag.TotalPaidAmount = totalPaidAmount;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.ConfirmedOrders = confirmedOrders;
            ViewBag.RecentTransactions = recentTransactions;
            ViewBag.TotalOrders = sellerOrders.Count;

            return View("~/Views/Wallet/SellerWallet.cshtml", sellerOrders);
        }

        // GET: Wallet/Balance (API endpoint)
        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await GetOrCreateWalletAsync(userId);
            return Json(new { balance = wallet.Balance });
        }

        private async Task<CoinWallet> GetOrCreateWalletAsync(string userId)
        {
            var wallet = await _context.CoinWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new CoinWallet
                {
                    UserId = userId,
                    Balance = 0m,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CoinWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            return wallet;
        }
    }
}
