using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Data;
using MultiTenantBlog.Models;
using MultiTenantBlog.Services;

namespace MultiTenantBlog.Controllers
{
    /// <summary>
    /// Admin paneli controller'ı - Blog yönetimi için
    /// DÜZELTME: Model binding ve validation sorunları çözüldü
    /// </summary>
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ITenantService tenantService, ApplicationDbContext context) 
            : base(tenantService)
        {
            _context = context;
        }

        /// <summary>
        /// Admin ana sayfası - Dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // İstatistikleri hesapla
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

            // Son yazıları getir
            var recentPosts = await _context.BlogPosts
                .Where(p => p.TenantId == CurrentTenant.Id)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PublishedDate)
                .Take(5)
                .ToListAsync();

            // Kategorileri getir
            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .Include(c => c.BlogPosts) // Blog post sayısı için
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Stats = stats;
            ViewBag.RecentPosts = recentPosts;
            ViewBag.Categories = categories;

            return View();
        }

        /// <summary>
        /// Blog yazılarını listele
        /// </summary>
        public async Task<IActionResult> Posts()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var posts = await _context.BlogPosts
                .Where(p => p.TenantId == CurrentTenant.Id)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            return View(posts);
        }

        // ======================== BLOG POST YÖNETİMİ ========================

        /// <summary>
        /// Yeni blog yazısı oluşturma sayfası (GET)
        /// DÜZELTME: ViewBag Categories düzgün yükleniyor
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreatePost()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // Kategorileri dropdown için hazırla
            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToListAsync();

            ViewBag.Categories = categories;

            // Boş model döndür
            var model = new BlogPost
            {
                Author = "Admin", // Default yazar
                IsPublished = true, // Default yayınlı
                PublishedDate = DateTime.Now
            };

            return View(model);
        }

        /// <summary>
        /// Yeni blog yazısı kaydetme (POST)
        /// DÜZELTME: TenantId otomatik atanıyor, model validation düzeltildi
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(BlogPost model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // TenantId'yi manuel ata (required field hatası çözümü)
            model.TenantId = CurrentTenant.Id;

            // Gerekli alanların kontrolü (ModelState'ten önce)
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("Title", "Yazı başlığı zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError("Content", "Yazı içeriği zorunludur.");
            }

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Kategori seçimi zorunludur.");
            }

            // Kategori var mı kontrol et
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId && c.TenantId == CurrentTenant.Id);
            
            if (model.CategoryId > 0 && !categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Seçilen kategori bulunamadı.");
            }

            // ModelState'i Tenant için temizle (çünkü manuel atadık)
            ModelState.Remove("Tenant");

            if (ModelState.IsValid)
            {
                try
                {
                    // Tarih ayarları
                    model.PublishedDate = DateTime.Now;
                    
                    // Eğer Summary boşsa, Content'in ilk 200 karakterini kullan
                    if (string.IsNullOrEmpty(model.Summary) && !string.IsNullOrEmpty(model.Content))
                    {
                        // HTML etiketlerini temizle
                        var plainText = System.Text.RegularExpressions.Regex.Replace(model.Content, "<.*?>", "");
                        model.Summary = plainText.Length > 200 
                            ? plainText.Substring(0, 200) + "..." 
                            : plainText;
                    }

                    _context.BlogPosts.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Blog yazısı başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Posts));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                }
            }

            // Hata durumunda kategorileri tekrar yükle
            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(model);
        }

        /// <summary>
        /// Blog yazısı düzenleme sayfası (GET)
        /// DÜZELTME: Mevcut kategori bilgisi ViewBag'e eklendi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var post = await _context.BlogPosts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == CurrentTenant.Id);

            if (post == null)
                return NotFound("Blog yazısı bulunamadı.");

            // Kategorileri dropdown için hazırla
            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = post.Category; // Mevcut kategori bilgisi

            return View(post);
        }

        /// <summary>
        /// Blog yazısı güncelleme (POST)
        /// DÜZELTME: Güncelleme işlemi düzeltildi
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, BlogPost model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            if (id != model.Id)
                return BadRequest("ID uyuşmazlığı.");

            var existingPost = await _context.BlogPosts
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == CurrentTenant.Id);

            if (existingPost == null)
                return NotFound("Blog yazısı bulunamadı.");

            // Model validation (aynı CreatePost gibi)
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError("Title", "Yazı başlığı zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError("Content", "Yazı içeriği zorunludur.");
            }

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Kategori seçimi zorunludur.");
            }

            // Kategori var mı kontrol et
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId && c.TenantId == CurrentTenant.Id);
            
            if (model.CategoryId > 0 && !categoryExists)
            {
                ModelState.AddModelError("CategoryId", "Seçilen kategori bulunamadı.");
            }

            // ModelState temizleme
            ModelState.Remove("Tenant");

            if (ModelState.IsValid)
            {
                try
                {
                    // Sadece değiştirilebilir alanları güncelle
                    existingPost.Title = model.Title;
                    existingPost.Content = model.Content;
                    existingPost.Summary = model.Summary;
                    existingPost.CategoryId = model.CategoryId;
                    existingPost.Author = model.Author;
                    existingPost.IsPublished = model.IsPublished;
                    existingPost.UpdatedDate = DateTime.Now;

                    // Summary otomatik oluştur
                    if (string.IsNullOrEmpty(existingPost.Summary) && !string.IsNullOrEmpty(existingPost.Content))
                    {
                        var plainText = System.Text.RegularExpressions.Regex.Replace(existingPost.Content, "<.*?>", "");
                        existingPost.Summary = plainText.Length > 200 
                            ? plainText.Substring(0, 200) + "..." 
                            : plainText;
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi!";
                    return RedirectToAction(nameof(Posts));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                }
            }

            // Hata durumunda kategorileri tekrar yükle
            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .OrderBy(c => c.Name)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(model);
        }

        /// <summary>
        /// Blog yazısı silme
        /// DÜZELTME: Route parametresi düzeltildi
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

            try
            {
                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"'{post.Title}' yazısı başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Yazı silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Posts));
        }

        // ======================== KATEGORİ YÖNETİMİ ========================

        /// <summary>
        /// Kategorileri listele
        /// DÜZELTME: Include ilişkiler düzeltildi
        /// </summary>
        public async Task<IActionResult> Categories()
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var categories = await _context.Categories
                .Where(c => c.TenantId == CurrentTenant.Id)
                .Include(c => c.BlogPosts.Where(p => p.IsPublished)) // Sadece yayınlanan yazılar
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

            var model = new Category
            {
                Color = "#007bff" // Default renk
            };

            return View(model);
        }

        /// <summary>
        /// Yeni kategori kaydetme (POST)
        /// DÜZELTME: TenantId otomatik atama ve validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category model)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            // TenantId'yi manuel ata
            model.TenantId = CurrentTenant.Id;

            // Custom validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Kategori adı zorunludur.");
            }

            if (string.IsNullOrWhiteSpace(model.Color))
            {
                model.Color = "#007bff";
            }

            // Aynı isimde kategori var mı kontrol et
            var existingCategory = await _context.Categories
                .AnyAsync(c => c.TenantId == CurrentTenant.Id && 
                              c.Name.ToLower() == model.Name.ToLower());

            if (existingCategory)
            {
                ModelState.AddModelError("Name", "Bu isimde bir kategori zaten var.");
            }

            // ModelState temizleme
            ModelState.Remove("Tenant");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Categories.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"'{model.Name}' kategorisi başarıyla oluşturuldu!";
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                }
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
                .Include(c => c.BlogPosts)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == CurrentTenant.Id);

            if (category == null)
                return NotFound("Kategori bulunamadı.");

            return View(category);
        }

        /// <summary>
        /// Kategori güncelleme (POST)
        /// DÜZELTME: Güncelleme işlemi düzeltildi
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

            // Custom validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Kategori adı zorunludur.");
            }

            // Aynı isimde başka kategori var mı kontrol et
            var duplicateCategory = await _context.Categories
                .AnyAsync(c => c.TenantId == CurrentTenant.Id && 
                              c.Id != id &&
                              c.Name.ToLower() == model.Name.ToLower());

            if (duplicateCategory)
            {
                ModelState.AddModelError("Name", "Bu isimde başka bir kategori zaten var.");
            }

            // ModelState temizleme
            ModelState.Remove("Tenant");

            if (ModelState.IsValid)
            {
                try
                {
                    existingCategory.Name = model.Name;
                    existingCategory.Description = model.Description;
                    existingCategory.Color = model.Color;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"'{model.Name}' kategorisi başarıyla güncellendi!";
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Kategori silme
        /// DÜZELTME: Route parametresi düzeltildi
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (CurrentTenant == null)
                return RedirectToAction("TenantNotFound", "Error");

            var category = await _context.Categories
                .Include(c => c.BlogPosts)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == CurrentTenant.Id);

            if (category == null)
                return NotFound("Kategori bulunamadı.");

            // Yazısı olan kategori silinemez
            if (category.BlogPosts.Any())
            {
                TempData["ErrorMessage"] = $"'{category.Name}' kategorisine ait {category.BlogPosts.Count} adet blog yazısı bulunuyor. Önce yazıları başka kategoriye taşıyın veya silin.";
                return RedirectToAction(nameof(Categories));
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"'{category.Name}' kategorisi başarıyla silindi!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategori silinirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Categories));
        }

        // ======================== YARDIMCI METODLAR ========================

        /// <summary>
        /// Tenant istatistiklerini JSON olarak döndür (AJAX için)
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
        /// Blog yazısının yayın durumunu değiştir (AJAX)
        /// DÜZELTME: AJAX route düzeltildi
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

            try
            {
                post.IsPublished = !post.IsPublished;
                post.UpdatedDate = DateTime.Now;

                if (post.IsPublished)
                {
                    post.PublishedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                var message = post.IsPublished ? "Yazı yayınlandı" : "Yazı taslağa çevrildi";
                return Json(new { success = true, message = message, isPublished = post.IsPublished });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
}