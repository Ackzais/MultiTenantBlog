using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Data;
using MultiTenantBlog.Services;

var builder = WebApplication.CreateBuilder(args);

// Services'leri container'a ekleyin (Dependency Injection)

// MVC servislerini ekle
builder.Services.AddControllersWithViews();

// Entity Framework ve SQL Server bağlantısını ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Multi-tenant servisini ekle
builder.Services.AddScoped<ITenantService, TenantService>();

var app = builder.Build();

// ⭐ SEED DATA - Geliştirme ortamında test verisi ekle
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            // Veritabanını test verisiyle doldur
            await SeedData.InitializeAsync(services);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Seed data oluşturulurken bir hata oluştu.");
        }
    }
}

// HTTP request pipeline'ını konfigüre edin

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // HTTP'yi HTTPS'e yönlendir
app.UseStaticFiles(); // CSS, JS, resim dosyalarını servis et

app.UseRouting(); // Route'ları etkinleştir

app.UseAuthorization(); // Yetkilendirmeyi etkinleştir

// Controller route'larını tanımla
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run(); // Uygulamayı çalıştır