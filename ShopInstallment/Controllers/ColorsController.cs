using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class ColorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ColorsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var items = await _context.Colors.OrderByDescending(c => c.Id).ToListAsync();
            return View(items);
        }

        public IActionResult Create() => View(new Color());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Color model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Colors.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Colors.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Color model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Colors.FindAsync(id);
            if (item == null) return NotFound();
            _context.Colors.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
