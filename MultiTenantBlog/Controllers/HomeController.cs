using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Data;
using MultiTenantBlog.Models;
using MultiTenantBlog.Services;
using System.Diagnostics;

namespace MultiTenantBlog.Controllers
{
    /// <summary>
    /// Ana sayfa ve blog listesi controller'ı
    /// </summary>
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;
        
        public HomeController(ITenantService tenantService, ApplicationDbContext context) 
            : base(tenantService)
        {
            _context = context;
        }
        
        /// <summary>
        /// Ana sayfa - Mevcut tenant'ın blog yazılarını listeler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");
                
            // Mevcut tenant'ın yayınlanmış yazılarını getir
            var posts = await _context.BlogPosts
                .Where(p => p.TenantId == CurrentTenant.Id && p.IsPublished)
                .Include(p => p.Category) // Category bilgisini de dahil et
                .OrderByDescending(p => p.PublishedDate) // Son yazılar önce
                .Take(10) // İlk 10 yazı
                .ToListAsync();
                
            return View(posts);
        }
        
        /// <summary>
        /// Blog yazısı detay sayfası
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");
                
            var post = await _context.BlogPosts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => 
                    p.Id == id && 
                    p.TenantId == CurrentTenant.Id && 
                    p.IsPublished);
                    
            if (post == null)
                return NotFound();
                
            return View(post);
        }
        
        /// <summary>
        /// Kategoriye göre yazıları listele
        /// </summary>
        public async Task<IActionResult> Category(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");
                
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == CurrentTenant.Id);
                
            if (category == null)
                return NotFound();
                
            var posts = await _context.BlogPosts
                .Where(p => p.CategoryId == id && p.TenantId == CurrentTenant.Id && p.IsPublished)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();
                
            ViewBag.Category = category;
            return View("Index", posts);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}