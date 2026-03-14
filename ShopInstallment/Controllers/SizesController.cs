using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class SizesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SizesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var items = await _context.Sizes.OrderByDescending(c => c.Id).ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(new Size());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Size model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Sizes.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Sizes.FindAsync(id);
            if (item == null) return NotFound();
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Size model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Sizes.FindAsync(id);
            if (item == null) return NotFound();
            _context.Sizes.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
