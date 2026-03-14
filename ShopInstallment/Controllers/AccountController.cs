using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // สลับมุมมองระหว่างผู้ซื้อและผู้ขาย
        [HttpGet]
        public IActionResult SwitchViewMode(string mode)
        {
            // ตรวจสอบว่าเป็น Seller หรือ Admin
            if (!User.IsInRole("Seller") && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "คุณไม่มีสิทธิ์ใช้งานฟีเจอร์นี้";
                return RedirectToAction("Index", "Home");
            }

            // เก็บค่า mode ใน session
            if (mode == "buyer" || mode == "seller")
            {
                HttpContext.Session.SetString("ViewMode", mode);
                TempData["Success"] = mode == "buyer" 
                    ? "เปลี่ยนเป็นมุมมองผู้ซื้อแล้ว" 
                    : "เปลี่ยนเป็นมุมมองผู้ขายแล้ว";
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: แสดงหน้าสมัครเป็นผู้ขาย
        [HttpGet]
        public async Task<IActionResult> BecomeSeller()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // ตรวจสอบว่าเป็น Seller หรือ Admin อยู่แล้วหรือไม่
            var isSeller = await _userManager.IsInRoleAsync(user, "Seller");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isSeller || isAdmin)
            {
                TempData["Error"] = "คุณเป็นผู้ขายหรือแอดมินอยู่แล้ว";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: ยืนยันการสมัครเป็นผู้ขาย
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeSeller(string agreement)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // ตรวจสอบว่าเป็น Seller หรือ Admin อยู่แล้วหรือไม่
            var isSeller = await _userManager.IsInRoleAsync(user, "Seller");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isSeller || isAdmin)
            {
                TempData["Error"] = "คุณเป็นผู้ขายหรือแอดมินอยู่แล้ว";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(agreement))
            {
                ModelState.AddModelError("", "กรุณายอมรับข้อตกลงและเงื่อนไข");
                return View();
            }

            // ตรวจสอบและสร้าง Role "Seller" หากยังไม่มี
            if (!await _roleManager.RoleExistsAsync("Seller"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Seller"));
            }

            // เพิ่ม Role "Seller" ให้กับผู้ใช้
            var result = await _userManager.AddToRoleAsync(user, "Seller");

            if (result.Succeeded)
            {
                // Refresh sign-in เพื่ออัพเดท claims
                await _signInManager.RefreshSignInAsync(user);

                TempData["Success"] = "🎉 ยินดีด้วย! คุณได้เป็นผู้ขายแล้ว สามารถเริ่มเพิ่มสินค้าได้เลย";
                return RedirectToAction("Index", "Products");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
        }

        // GET: แสดงโปรไฟล์ผู้ใช้
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;

            return View(user);
        }
    }
}
