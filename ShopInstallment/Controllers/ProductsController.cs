using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const int MaxImages = 5;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Color)
                .Include(p => p.Size)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await LoadLookups();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? mainImage, List<IFormFile>? images)
        {
            await LoadLookups();
            if (!ModelState.IsValid) return View(model);

            // ตรวจสอบรูปหลัก
            if (mainImage == null)
            {
                ModelState.AddModelError("", "กรุณาอัพโหลดรูปหลัก");
                return View(model);
            }

            // นับรวมรูปทั้งหมด (รูปหลัก + รูปเพิ่มเติม)
            images ??= new List<IFormFile>();
            int totalImages = 1 + images.Count;
            
            if (totalImages > MaxImages)
            {
                ModelState.AddModelError("", $"อัพโหลดรูปภาพได้สูงสุด {MaxImages} รูป (รวมรูปหลัก)");
                return View(model);
            }

            model.SellerId = User?.Identity?.Name ?? "unknown";
            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            // Save images
            var uploadsPath = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsPath);

            // บันทึกรูปหลักก่อน (DisplayOrder = 1, IsMain = true)
            var mainFileName = $"p_{model.Id}_main_{Guid.NewGuid():N}{Path.GetExtension(mainImage.FileName)}";
            var mainFilePath = Path.Combine(uploadsPath, mainFileName);
            using (var stream = System.IO.File.Create(mainFilePath))
            {
                await mainImage.CopyToAsync(stream);
            }
            var mainUrl = $"/images/products/{mainFileName}";
            var mainImageRecord = new ProductImage
            {
                ProductId = model.Id,
                ImageUrl = mainUrl,
                DisplayOrder = 1,
                IsMain = true
            };
            _context.ProductImages.Add(mainImageRecord);
            model.MainImage = mainUrl;

            // บันทึกรูปเพิ่มเติม (DisplayOrder = 2, 3, 4, 5)
            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                var fileName = $"p_{model.Id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                var url = $"/images/products/{fileName}";
                var pi = new ProductImage
                {
                    ProductId = model.Id,
                    ImageUrl = url,
                    DisplayOrder = i + 2,
                    IsMain = false
                };
                _context.ProductImages.Add(pi);
            }
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "เพิ่มสินค้าสำเร็จ";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            await LoadLookups();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, List<IFormFile>? images, int? mainIndex)
        {
            if (id != model.Id) return BadRequest();
            await LoadLookups();
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return NotFound();

            // Update fields
            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.PricePerUnit = model.PricePerUnit;
            existing.RentPricePerDay = model.RentPricePerDay;
            existing.Stock = model.Stock;
            existing.Status = model.Status;
            existing.CategoryId = model.CategoryId;
            existing.BrandId = model.BrandId;
            existing.ColorId = model.ColorId;
            existing.SizeId = model.SizeId;
            existing.UpdatedAt = DateTime.UtcNow;

            // Handle new images (append until max 5)
            images ??= new List<IFormFile>();
            if (existing.Images.Count + images.Count > MaxImages)
            {
                ModelState.AddModelError("", $"สินค้ามีรูปอยู่ {existing.Images.Count} รูป สามารถเพิ่มได้อีก {MaxImages - existing.Images.Count} รูปเท่านั้น");
                return View(existing);
            }

            var uploadsPath = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsPath);

            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                var fileName = $"p_{existing.Id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                var url = $"/images/products/{fileName}";
                var pi = new ProductImage
                {
                    ProductId = existing.Id,
                    ImageUrl = url,
                    DisplayOrder = existing.Images.Count + i + 1,
                    IsMain = false
                };
                _context.ProductImages.Add(pi);
            }

            // Set main image
            if (mainIndex.HasValue)
            {
                foreach (var img in existing.Images)
                    img.IsMain = false;
                var setIndex = mainIndex.Value;
                var ordered = existing.Images.OrderBy(x => x.DisplayOrder).ToList();
                if (setIndex >= 0 && setIndex < ordered.Count)
                {
                    ordered[setIndex].IsMain = true;
                    existing.MainImage = ordered[setIndex].ImageUrl;
                }
            }
            else if (!existing.Images.Any(i => i.IsMain) && existing.Images.Any())
            {
                var first = existing.Images.OrderBy(i => i.DisplayOrder).First();
                first.IsMain = true;
                existing.MainImage = first.ImageUrl;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _context.ProductImages.FindAsync(id);
            if (img == null) return NotFound();
            var product = await _context.Products.FindAsync(img.ProductId);
            _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();
            // adjust main image if deleted
            if (product != null && product.MainImage == img.ImageUrl)
            {
                var remain = await _context.ProductImages.Where(x => x.ProductId == product.Id).OrderBy(x => x.DisplayOrder).ToListAsync();
                foreach (var r in remain) r.IsMain = false;
                if (remain.Any())
                {
                    remain.First().IsMain = true;
                    product.MainImage = remain.First().ImageUrl;
                }
                else
                {
                    product.MainImage = null;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Edit), new { id = img.ProductId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            _context.ProductImages.RemoveRange(product.Images);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return NotFound();
            item.Status = status;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadLookups()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Brands = await _context.Brands.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Colors = await _context.Colors.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Sizes = await _context.Sizes.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
