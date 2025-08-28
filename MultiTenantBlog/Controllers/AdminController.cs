using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Data;
using MultiTenantBlog.Models;
using MultiTenantBlog.Services;

namespace MultiTenantBlog.Controllers
{
    /// <summary>
    /// Admin paneli controller'ı - Blog yönetimi için
    /// BaseController'dan miras alır, böylece CurrentTenant otomatik olarak set edilir
    /// </summary>
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ITenantService tenantService, ApplicationDbContext context) 
            : base(tenantService)
        {
            _context = context; // Database context'ini dependency injection ile alır
        }

        /// <summary>
        /// Admin ana sayfası - Dashboard
        /// Mevcut tenant'ın istatistiklerini ve son yazılarını gösterir
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // Mevcut tenant için istatistikleri hesapla
            var stats = new
            {
                TotalPosts = await _context.BlogPosts
                    .CountAsync(p => p.TenantId == CurrentTenant.Id),
                PublishedPosts = await _context.BlogPosts
                    .CountAsync(p => p.TenantId == CurrentTenant.Id && p.IsPublished),
                DraftPosts = await _context.BlogPosts
                    .CountAsync(p => p.TenantId == CurrentTenant.Id && !p.IsPublished),
                TotalCategories = await _context.Categories
                    .CountAsync(c => c.TenantId == CurrentTenant.Id)
            };

            // Son yazıları getir (5 tane)
            var recentPosts = await _context.BlogPosts
                .Where(p => p.TenantId == CurrentTenant.Id)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PublishedDate)
                .Take(5)
                .ToListAsync();

            // Kategorileri getir
            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ViewBag ile view'a gönder
            ViewBag.Stats = stats;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.Categories = categories;

            return View();
        }

        /// <summary>
        /// Tüm blog yazılarını listele (sadece mevcut tenant'a ait olanlar)
        /// </summary>
        public async Task<IActionResult> Posts()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var posts = await _context.BlogPosts
                .Where(p => p.TenantId == CurrentTenant.Id)
                .Include(p => p.Category) // Kategori bilgisini de getir
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            return View(posts);
        }

        /// <summary>
        /// Yeni blog yazısı oluşturma sayfası (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreatePost()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // Dropdown için kategorileri hazırla
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View();
        }

        /// <summary>
        /// Yeni blog yazısı kaydetme (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma
        public async Task<IActionResult> CreatePost(BlogPost model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // Model validation kontrolü
            if (ModelState.IsValid)
            {
                // Yeni blog yazısını hazırla
                model.TenantId = CurrentTenant.Id;
                model.PublishedDate = DateTime.Now;
                
                // Eğer Summary (özet) boşsa, Content'in ilk 200 karakterini kullan
                if (string.IsNullOrEmpty(model.Summary) && !string.IsNullOrEmpty(model.Content))
                {
                    var plainText = System.Text.RegularExpressions.Regex.Replace(model.Content, "<.*?>", "");
                    model.Summary = plainText.Length > 200 
                        ? plainText.Substring(0, 200) + "..." 
                        : plainText;
                }

                // Veritabanına ekle
                _context.BlogPosts.Add(model);
                await _context.SaveChangesAsync();

                // Başarı mesajı
                TempData["SuccessMessage"] = "Blog yazısı başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Posts));
            }

            // Hata durumunda kategorileri tekrar yükle
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(model);
        }

        /// <summary>
        /// Blog yazısı düzenleme sayfası (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == CurrentTenant.Id);

            if (post == null)
                return NotFound("Blog yazısı bulunamadı.");

            // Kategorileri dropdown için hazırla
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(post);
        }

        /// <summary>
        /// Blog yazısı güncelleme (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, BlogPost model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            if (id != model.Id)
                return BadRequest("ID uyuşmazlığı.");

            // Mevcut blog yazısını bul
            var existingPost = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == CurrentTenant.Id);

            if (existingPost == null)
                return NotFound("Blog yazısı bulunamadı.");

            if (ModelState.IsValid)
            {
                // Mevcut post'u güncelle (sadece değiştirilebilir alanlar)
                existingPost.Title = model.Title;
                existingPost.Content = model.Content;
                existingPost.Summary = model.Summary;
                existingPost.CategoryId = model.CategoryId;
                existingPost.Author = model.Author;
                existingPost.IsPublished = model.IsPublished;
                existingPost.UpdatedDate = DateTime.Now;

                // Eğer Summary boşsa, Content'in ilk 200 karakterini kullan
                if (string.IsNullOrEmpty(existingPost.Summary) && !string.IsNullOrEmpty(existingPost.Content))
                {
                    var plainText = System.Text.RegularExpressions.Regex.Replace(existingPost.Content, "<.*?>", "");
                    existingPost.Summary = plainText.Length > 200 
                        ? plainText.Substring(0, 200) + "..." 
                        : plainText;
                }

                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi!";
                return RedirectToAction(nameof(Posts));
            }

            // Hata durumunda kategorileri tekrar yükle
            ViewBag.Categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(model);
        }

        /// <summary>
        /// Blog yazısı silme
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == CurrentTenant.Id);

            if (post == null)
                return NotFound("Blog yazısı bulunamadı.");

            // Blog yazısını sil
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog yazısı başarıyla silindi!";
            return RedirectToAction(nameof(Posts));
        }

        // ======================== KATEGORİ YÖNETİMİ ========================

        /// <summary>
        /// Kategorileri listele
        /// </summary>
        public async Task<IActionResult> Categories()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .Include(c => c.BlogPosts) // Her kategorinin kaç yazısı var
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        /// <summary>
        /// Yeni kategori oluşturma sayfası (GET)
        /// </summary>
        [HttpGet]
        public IActionResult CreateCategory()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            return View();
        }

        /// <summary>
        /// Yeni kategori kaydetme (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            if (ModelState.IsValid)
            {
                model.TenantId = CurrentTenant.Id;
                
                // Kategori rengi belirtilmemişse varsayılan renk ata
                if (string.IsNullOrEmpty(model.Color))
                {
                    model.Color = "#007bff"; // Bootstrap primary color
                }

                _context.Categories.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Categories));
            }

            return View(model);
        }

        /// <summary>
        /// Kategori düzenleme sayfası (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == CurrentTenant.Id);

            if (category == null)
                return NotFound("Kategori bulunamadı.");

            return View(category);
        }

        /// <summary>
        /// Kategori güncelleme (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            if (id != model.Id)
                return BadRequest("ID uyuşmazlığı.");

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == CurrentTenant.Id);

            if (existingCategory == null)
                return NotFound("Kategori bulunamadı.");

            if (ModelState.IsValid)
            {
                existingCategory.Name = model.Name;
                existingCategory.Description = model.Description;
                existingCategory.Color = model.Color;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
                return RedirectToAction(nameof(Categories));
            }

            return View(model);
        }

        /// <summary>
        /// Kategori silme
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var category = await _context.Categories
                .Include(c => c.BlogPosts) // Bu kategoriye ait yazıları da getir
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == CurrentTenant.Id);

            if (category == null)
                return NotFound("Kategori bulunamadı.");

            // Eğer kategoriye ait yazılar varsa silme işlemini engelle
            if (category.BlogPosts.Any())
            {
                TempData["ErrorMessage"] = $"'{category.Name}' kategorisine ait {category.BlogPosts.Count} adet blog yazısı bulunuyor. Önce bu yazıları başka kategoriye taşıyın veya silin.";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kategori başarıyla silindi!";
            return RedirectToAction(nameof(Categories));
        }

        // ======================== YARDIMCI METODLAR ========================

        /// <summary>
        /// Mevcut tenant'ın istatistiklerini JSON olarak döndürür (AJAX için)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            if (CurrentTenant == null)
                return Json(new { error = "Tenant bulunamadı" });

            var stats = new
            {
                totalPosts = await _context.BlogPosts.CountAsync(p => p.TenantId == CurrentTenant.Id),
                publishedPosts = await _context.BlogPosts.CountAsync(p => p.TenantId == CurrentTenant.Id && p.IsPublished),
                draftPosts = await _context.BlogPosts.CountAsync(p => p.TenantId == CurrentTenant.Id && !p.IsPublished),
                totalCategories = await _context.Categories.CountAsync(c => c.TenantId == CurrentTenant.Id),
                tenantInfo = new
                {
                    name = CurrentTenant.Name,
                    domain = CurrentTenant.Domain,
                    theme = CurrentTenant.Theme
                }
            };

            return Json(stats);
        }

        /// <summary>
        /// Blog yazısının yayın durumunu değiştir (Published/Draft)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublishStatus(int id)
        {
            if (CurrentTenant == null)
                return Json(new { success = false, message = "Tenant bulunamadı" });

            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == CurrentTenant.Id);

            if (post == null)
                return Json(new { success = false, message = "Blog yazısı bulunamadı" });

            // Yayın durumunu tersine çevir.
            post.IsPublished = !post.IsPublished;
            post.UpdatedDate = DateTime.Now;

            // Eğer yayından kaldırılıyorsa PublishedDate'i güncelle
            if (post.IsPublished)
            {
                post.PublishedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            var message = post.IsPublished ? "Yazı yayınlandı" : "Yazı taslağa çevrildi";
            return Json(new { success = true, message = message, isPublished = post.IsPublished });
        }
    }
}