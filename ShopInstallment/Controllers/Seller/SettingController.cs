using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;
using System.Security.Claims;

namespace ShopInstallment.Controllers.Seller
{
    [Authorize(Roles = "Seller,Admin")]
    public class SellerSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SellerSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: SellerSettings
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var setting = await _context.SellerPaymentSettings
                .FirstOrDefaultAsync(s => s.SellerId == userId);

            if (setting == null)
            {
                // สร้างการตั้งค่าเริ่มต้น
                setting = new SellerPaymentSetting
                {
                    SellerId = userId,
                    AllowPromptPay = true,
                    PromptPayFeePercent = 1.5m,
                    AllowBillPayment = false,
                    BillPaymentFeePercent = 2.0m,
                    AllowTrueWallet = false,
                    TrueWalletFeePercent = 1.8m,
                    AllowBankTransfer = true,
                    BankTransferFeePercent = 0m,
                    AllowCoin = true,
                    CoinFeePercent = 0.5m
                };
                _context.SellerPaymentSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return View(setting);
        }

        // GET: SellerSettings/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var setting = await _context.SellerPaymentSettings
                .FirstOrDefaultAsync(s => s.SellerId == userId);

            if (setting == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(setting);
        }

        // POST: SellerSettings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SellerPaymentSetting model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var setting = await _context.SellerPaymentSettings
                .FirstOrDefaultAsync(s => s.SellerId == userId);

            if (setting == null)
            {
                return NotFound();
            }

            // อัพเดทการตั้งค่า
            setting.AllowPromptPay = model.AllowPromptPay;
            setting.PromptPayFeePercent = model.PromptPayFeePercent;
            setting.AllowBillPayment = model.AllowBillPayment;
            setting.BillPaymentFeePercent = model.BillPaymentFeePercent;
            setting.AllowTrueWallet = model.AllowTrueWallet;
            setting.TrueWalletFeePercent = model.TrueWalletFeePercent;
            setting.AllowBankTransfer = model.AllowBankTransfer;
            setting.BankTransferFeePercent = model.BankTransferFeePercent;
            setting.AllowCoin = model.AllowCoin;
            setting.CoinFeePercent = model.CoinFeePercent;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "บันทึกการตั้งค่าเรียบร้อยแล้ว";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "เกิดข้อผิดพลาด: " + ex.Message);
                return View(model);
            }
        }
    }
}
