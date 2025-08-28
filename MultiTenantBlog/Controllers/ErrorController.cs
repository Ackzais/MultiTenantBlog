using Microsoft.AspNetCore.Mvc;

namespace MultiTenantBlog.Controllers
{
    /// <summary>
    /// Hata yönetimi için controller
    /// </summary>
    public class ErrorController : Controller
    {
        /// <summary>
        /// Tenant bulunamadığında gösterilen sayfa
        /// </summary>
        public IActionResult TenantNotFound()
        {
            ViewBag.ErrorMessage = "Bu domain için blog bulunamadı.";
            ViewBag.ErrorDetails = "Geçerli bir blog domain'i kullandığınızdan emin olun.";
            
            return View();
        }
        
        /// <summary>
        /// Genel hata sayfası
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.ErrorMessage = "Bir hata oluştu.";
            ViewBag.ErrorDetails = "Lütfen daha sonra tekrar deneyin.";
            
            return View();
        }
    }
}