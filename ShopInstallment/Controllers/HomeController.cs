using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInstallment.Data;
using ShopInstallment.Models;

namespace ShopInstallment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // ดึง products ล่าสุด 12 รายการ
            var products = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Ratings)
                .Where(p => p.Status == "Active")
                .OrderByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            // ดึง categories
            var categories = await _context.Categories
                .Where(c => c.Status == "Active")
                .ToListAsync();

            ViewBag.Categories = categories;
            return View(products);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
