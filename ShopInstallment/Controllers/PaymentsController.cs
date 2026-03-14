using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;
using ShopInstallment.Services.Payments;
using System.Security.Claims;

namespace ShopInstallment.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentProvider _provider;
        public PaymentsController(ApplicationDbContext context, IPaymentProvider provider)
        {
            _context = context;
            _provider = provider;
        }

        // GET: Show payment page
        public async Task<IActionResult> Create(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            
            if (order == null) return NotFound();

            var setting = await _context.SellerPaymentSettings.FirstOrDefaultAsync(s => s.SellerId == order.CustomerId);
            if (setting == null)
            {
                setting = new SellerPaymentSetting { SellerId = order.CustomerId };
                _context.SellerPaymentSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await _context.CoinWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            var coinBalance = wallet?.Balance ?? 0;

            ViewBag.SellerSetting = setting;
            ViewBag.CoinBalance = coinBalance;

            return View(order);
        }

        // POST: Process payment
        [HttpPost]
        public async Task<IActionResult> Create(int orderId, string method)
        {
            var order = await _context.Orders.Include(o => o.Payments).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();
            if (!Enum.TryParse<PaymentMethod>(method, true, out var pm)) return BadRequest("invalid method");

            // seller settings
            var sellerId = order.CustomerId; // example: can be product seller or marketplace logic
            var setting = await _context.SellerPaymentSettings.FirstOrDefaultAsync(s => s.SellerId == sellerId);
            if (setting == null)
            {
                setting = new SellerPaymentSetting { SellerId = sellerId };
                _context.SellerPaymentSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            bool allowed = pm switch
            {
                PaymentMethod.PromptPay => setting.AllowPromptPay,
                PaymentMethod.BillPayment => setting.AllowBillPayment,
                PaymentMethod.TrueWallet => setting.AllowTrueWallet,
                PaymentMethod.BankTransfer => setting.AllowBankTransfer,
                PaymentMethod.Coin => setting.AllowCoin,
                _ => false
            };
            if (!allowed) return BadRequest("method not allowed by seller");

            var amount = order.RemainingAmount;
            
            // ถ้าเลือกจ่ายด้วย Coin ให้ตรวจสอบและหักยอด
            if (pm == PaymentMethod.Coin)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var wallet = await _context.CoinWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                
                if (wallet == null || wallet.Balance < amount)
                {
                    TempData["Error"] = "ยอด Coin ไม่เพียงพอ กรุณาเติมเงินก่อน";
                    return RedirectToAction("Details", "Orders", new { id = order.Id });
                }
                
                // หักยอด Coin
                wallet.Balance -= amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                
                // บันทึกธุรกรรม Coin
                var coinTransaction = new CoinTransaction
                {
                    UserId = userId,
                    Amount = -amount,
                    Type = "Spend",
                    Reference = $"Order #{order.OrderNumber}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.CoinTransactions.Add(coinTransaction);
            }
            
            var txId = pm == PaymentMethod.Coin 
                ? $"COIN-{DateTime.UtcNow.Ticks}" 
                : await _provider.CreateChargeAsync(order, amount, pm, new Dictionary<string, string> { { "orderId", orderId.ToString() } });

            var feePercent = pm switch
            {
                PaymentMethod.PromptPay => setting.PromptPayFeePercent,
                PaymentMethod.BillPayment => setting.BillPaymentFeePercent,
                PaymentMethod.TrueWallet => setting.TrueWalletFeePercent,
                PaymentMethod.BankTransfer => setting.BankTransferFeePercent,
                PaymentMethod.Coin => setting.CoinFeePercent,
                _ => 0m
            };
            var fee = Math.Round(amount * feePercent / 100m, 2);

            var payment = new Payment
            {
                OrderId = order.Id,
                TransactionId = txId,
                Amount = amount,
                FeeAmount = fee,
                PaymentMethod = pm.ToString(),
                Status = pm == PaymentMethod.Coin ? "Completed" : "EscrowHold"
            };
            _context.Payments.Add(payment);
            
            // อัปเดตยอดชำระของ Order
            order.PaidAmount += amount;
            order.RemainingAmount = order.TotalAmount - order.PaidAmount;
            if (order.RemainingAmount <= 0)
            {
                order.Status = "Paid";
            }
            
            await _context.SaveChangesAsync();

            TempData["Success"] = pm == PaymentMethod.Coin 
                ? $"ชำระด้วย Coin สำเร็จ {amount:N2} Coin" 
                : "ดำเนินการชำระเงินสำเร็จ";
            return RedirectToAction("Status", "Payments", new { id = payment.Id });
        }

        // GET: Show payment status
        public async Task<IActionResult> Status(int id)
        {
            var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/Admin/Payments")]
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Customer)
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.Seller)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View("~/Views/Admin/Payments/Index.cshtml", payments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/Admin/Payments/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (payment == null) return NotFound();
            return View("~/Views/Admin/Payments/Details.cshtml", payment);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ReleaseEscrow(int paymentId)
        {
            var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return NotFound();
            payment.Status = "EscrowReleased";
            await _context.SaveChangesAsync();
            TempData["Success"] = "ปล่อยเงินสำเร็จ";
            return RedirectToAction("Details", new { id = payment.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> FreezeEscrow(int paymentId)
        {
            var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return NotFound();
            payment.Status = "EscrowFrozen";
            await _context.SaveChangesAsync();
            TempData["Success"] = "ระงับเงินสำเร็จ";
            return RedirectToAction("Details", new { id = payment.Id });
        }

        // Old admin methods for order details page
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Payments/ReleaseEscrowFromOrder")]
        public async Task<IActionResult> ReleaseEscrowFromOrder(int paymentId)
        {
            var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return NotFound();
            payment.Status = "EscrowReleased";
            await _context.SaveChangesAsync();
            TempData["Success"] = "ปล่อยเงินสำเร็จ";
            return RedirectToAction("Details", "Orders", new { id = payment.OrderId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Payments/FreezeEscrowFromOrder")]
        public async Task<IActionResult> FreezeEscrowFromOrder(int paymentId)
        {
            var payment = await _context.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return NotFound();
            payment.Status = "EscrowFrozen";
            await _context.SaveChangesAsync();
            TempData["Success"] = "ระงับเงินสำเร็จ";
            return RedirectToAction("Details", "Orders", new { id = payment.OrderId });
        }
    }
}
