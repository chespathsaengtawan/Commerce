using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var items = await _context.Categories.Include(c => c.Gender).OrderByDescending(c => c.CreatedAt).ToListAsync();

            ViewBag.Genders = await _context.Genders.ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Genders = await _context.Genders.ToListAsync();
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            // ลบ navigation properties validation
            ModelState.Remove("Gender");
            ModelState.Remove("Products");
            
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบ";
                ViewBag.Genders = await _context.Genders.ToListAsync();
                return View(model);
            }
            
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "เพิ่มหมวดหมู่สำเร็จ";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.Genders = await _context.Genders.ToListAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return BadRequest();
            
            // ลบ Gender validation error ออก เพราะ navigation property ไม่ได้มาจาก form
            ModelState.Remove("Gender");
            ModelState.Remove("Products");
            
            model.UpdatedAt = DateTime.UtcNow;
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบ";

                ViewBag.Genders = await _context.Genders.ToListAsync();
                return View(model);
            }

            // ดึง entity จาก database และ update เฉพาะ property ที่ต้องการ
            var item = await _context.Categories.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = model.Name;
            item.Description = model.Description;
            item.Status = model.Status;
            item.GenderId = model.GenderId;
            item.UpdatedAt = model.UpdatedAt;

            // EF Core จะ track changes อัตโนมัติ ไม่ต้อง Update() หรือ Entry().State
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "แก้ไขหมวดหมู่สำเร็จ";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null) return NotFound();
            _context.Categories.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "ลบหมวดหมู่สำเร็จ";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null) return NotFound();
            item.Status = status;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
