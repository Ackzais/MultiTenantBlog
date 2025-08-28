using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Data;
using MultiTenantBlog.Models;

namespace MultiTenantBlog.Services
{
    /// <summary>
    /// Tenant işlemlerini gerçekleştiren servis sınıfı
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        
        public TenantService(ApplicationDbContext context)
        {
            _context = context; // Database context'ini DI ile alır
        }
        
        /// <summary>
        /// Domain adına göre tenant bulur
        /// </summary>
        public async Task<Tenant?> GetTenantByDomainAsync(string domain)
        {
            // Domain'i küçük harfe çevir ve veritabanında ara
            var normalizedDomain = domain.ToLowerInvariant();
            
            return await _context.Tenants
                .Where(t => t.IsActive) // Sadece aktif tenant'lar
                .FirstOrDefaultAsync(t => 
                    t.Domain.ToLower() == normalizedDomain || 
                    (t.Subdomain != null && t.Subdomain.ToLower() == normalizedDomain));
        }
        
        /// <summary>
        /// Tüm aktif tenant'ları listeler
        /// </summary>
        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        
        /// <summary>
        /// HTTP isteğinden hangi tenant olduğunu belirler
        /// </summary>
        public async Task<Tenant?> GetCurrentTenantAsync(HttpContext httpContext)
        {
            // HTTP isteğinden host bilgisini al
            var host = httpContext.Request.Host.Host;
            
            // Development ortamında localhost:port olabilir
            if (host.StartsWith("localhost"))
            {
                // Geliştirme ortamında subdomain simülasyonu için query string kullan
                var tenantParam = httpContext.Request.Query["tenant"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantParam))
                {
                    return await GetTenantByDomainAsync(tenantParam);
                }
                
                // Default olarak ilk tenant'ı döndür
                return await _context.Tenants.FirstOrDefaultAsync(t => t.IsActive);
            }
            
            // Production'da gerçek domain ile tenant bul
            return await GetTenantByDomainAsync(host);
        }
    }
}