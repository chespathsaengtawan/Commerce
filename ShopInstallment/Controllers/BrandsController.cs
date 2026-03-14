using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BrandsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var items = await _context.Brands.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(items);
        }

        public IActionResult Create() => View(new Brand());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Brands.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            _context.Brands.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var item = await _context.Brands.FindAsync(id);
            if (item == null) return NotFound();
            item.Status = status;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
