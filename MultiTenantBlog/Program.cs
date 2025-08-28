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

// Multi-tenant servisini ekle (bu servisi bir sonraki adımda oluşturacağız)
builder.Services.AddScoped<ITenantService, TenantService>();

var app = builder.Build();

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