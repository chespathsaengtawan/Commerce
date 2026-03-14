using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RatingsController(ApplicationDbContext context) => _context = context;

        // List ratings for a product
        [AllowAnonymous]
        public async Task<IActionResult> Product(int productId)
        {
            var product = await _context.Products.Include(p => p.Ratings).FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();
            var ratings = await _context.ProductRatings
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            ViewBag.Product = product;
            ViewBag.Average = ratings.Any() ? ratings.Average(r => r.Stars) : 0;
            return View(ratings);
        }

        // Create rating for a product
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();
            ViewBag.Product = product;
            return View(new ProductRating { ProductId = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductRating model)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();
            // Prevent duplicate review from the same user for this product
            var currentUserId = User?.Identity?.Name ?? "anonymous";
            var already = await _context.ProductRatings
                .AnyAsync(r => r.ProductId == model.ProductId && r.UserId == currentUserId);
            if (already)
            {
                ModelState.AddModelError("", "คุณได้รีวิวสินค้านี้ไปแล้ว");
                ViewBag.Product = product;
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }
            model.UserId = currentUserId;
            model.CreatedAt = DateTime.UtcNow;
            _context.ProductRatings.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Product", new { productId = model.ProductId });
        }

        // Optional: allow delete by Admin or owner
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _context.ProductRatings.FindAsync(id);
            if (rating == null) return NotFound();
            var productId = rating.ProductId;
            _context.ProductRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return RedirectToAction("Product", new { productId });
        }
    }
}
