using MultiTenantBlog.Models;

namespace MultiTenantBlog.Services
{
    /// <summary>
    /// Tenant işlemleri için interface - Hangi metodların olacağını tanımlar
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Domain/subdomain'e göre tenant'ı bulur
        /// </summary>
        Task<Tenant?> GetTenantByDomainAsync(string domain);
        
        /// <summary>
        /// Tüm aktif tenant'ları getirir
        /// </summary>
        Task<List<Tenant>> GetAllTenantsAsync();
        
        /// <summary>
        /// HTTP isteğinden mevcut tenant'ı belirler
        /// </summary>
        Task<Tenant?> GetCurrentTenantAsync(HttpContext httpContext);
    }
}