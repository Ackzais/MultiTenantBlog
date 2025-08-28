using Microsoft.AspNetCore.Mvc;
using MultiTenantBlog.Services;

namespace MultiTenantBlog.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult TenantNotFound()
        {
            ViewBag.ErrorMessage = "Bu domain için blog bulunamadı.";
            return View();
        }
        
        public IActionResult Index()
        {
            return View();
        }
    }
}