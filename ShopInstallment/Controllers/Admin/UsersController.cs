using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Models;
using ShopInstallment.ViewModels;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("/Admin/Users")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var rows = new List<AdminUserRow>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var isLockedOut = await _userManager.IsLockedOutAsync(user);
                rows.Add(new AdminUserRow
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    IsLockedOut = isLockedOut,
                    Roles = roles
                });
            }

            var adminCount = rows.Count(r => r.IsAdmin);
            var sellerCount = rows.Count(r => r.IsSeller);
            var total = rows.Count;
            var customerCount = Math.Max(0, total - adminCount - sellerCount);

            var vm = new AdminUserListViewModel
            {
                Users = rows,
                TotalUsers = total,
                AdminUsers = adminCount,
                SellerUsers = sellerCount,
                CustomerUsers = customerCount
            };

            return View("~/Views/Admin/Users/Index.cshtml", vm);
        }

        [HttpGet("/Admin/Users/AddSeller")]
        public async Task<IActionResult> AddSeller(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขสิทธิ์ของผู้ดูแลระบบได้";
                return RedirectToAction(nameof(Index));
            }

            if (!await _roleManager.RoleExistsAsync("Seller"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Seller"));
            }

            if (!await _userManager.IsInRoleAsync(user, "Seller"))
            {
                await _userManager.AddToRoleAsync(user, "Seller");
            }

            TempData["SuccessMessage"] = "เพิ่มสิทธิ์ผู้ขายให้ผู้ใช้งานแล้ว";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Admin/Users/RemoveSeller")]
        public async Task<IActionResult> RemoveSeller(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขสิทธิ์ของผู้ดูแลระบบได้";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "Seller"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Seller");
            }

            TempData["SuccessMessage"] = "ลบสิทธิ์ผู้ขายออกจากผู้ใช้งานแล้ว";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Admin/Users/Lock")]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "ไม่สามารถระงับบัญชีผู้ดูแลระบบได้";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = _userManager.GetUserId(User);
            if (string.Equals(user.Id, currentUserId, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "ไม่สามารถระงับบัญชีตัวเองได้";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            await _userManager.UpdateSecurityStampAsync(user);

            TempData["SuccessMessage"] = "ระงับบัญชีผู้ใช้งานเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Admin/Users/Unlock")]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            TempData["SuccessMessage"] = "ปลดล็อกบัญชีผู้ใช้งานแล้ว";
            return RedirectToAction(nameof(Index));
        }
    }
}
