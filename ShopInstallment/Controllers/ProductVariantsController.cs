using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class ProductVariantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductVariantsController(ApplicationDbContext context) => _context = context;

        // List variants by product
        public async Task<IActionResult> Index(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();
            var variants = await _context.ProductVariants.Where(v => v.ProductId == productId).ToListAsync();
            ViewBag.Product = product;
            return View(variants);
        }

        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();
            ViewBag.Product = product;
            return View(new ProductVariant { ProductId = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVariant model)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }
            _context.ProductVariants.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return NotFound();
            var product = await _context.Products.FindAsync(variant.ProductId);
            ViewBag.Product = product;
            return View(variant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductVariant model)
        {
            if (id != model.Id) return BadRequest();
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return NotFound();
            if (!ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                ViewBag.Product = product;
                return View(model);
            }
            variant.VariantName = model.VariantName;
            variant.PriceAdjustment = model.PriceAdjustment;
            variant.Stock = model.Stock;
            variant.SKU = model.SKU;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { productId = model.ProductId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return NotFound();
            var productId = variant.ProductId;
            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { productId });
        }
    }
}
