using Microsoft.AspNetCore.Mvc;
using MultiTenantBlog.Models;
using MultiTenantBlog.Services;

namespace MultiTenantBlog.Controllers
{
    /// <summary>
    /// Tüm controller'ların miras alacağı base sınıf
    /// Tenant bilgisini otomatik olarak sağlar
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly ITenantService _tenantService;
        protected Tenant? CurrentTenant { get; private set; }
        
        public BaseController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }
        
        /// <summary>
        /// Her action'dan önce çalışır, mevcut tenant'ı belirler
        /// </summary>
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context, 
            ActionExecutionDelegate next)
        {
            // Mevcut tenant'ı bul
            CurrentTenant = await _tenantService.GetCurrentTenantAsync(HttpContext);
            
            // View'larda kullanılmak üzere ViewBag'e ekle
            ViewBag.CurrentTenant = CurrentTenant;
            
            // Eğer tenant bulunamadıysa hata sayfasına yönlendir
            if (CurrentTenant == null)
            {
                context.Result = RedirectToAction("TenantNotFound", "Error");
                return;
            }
            
            // Normal action execution'ı devam ettir
            await next();
        }
    }
}