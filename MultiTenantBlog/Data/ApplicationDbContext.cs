using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Models;

namespace MultiTenantBlog.Data
{
    /// <summary>
    /// Entity Framework Database Context - Veritabanı işlemlerini yönetir
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // Constructor - DI (Dependency Injection) ile options alır
        }
        
        // DbSet'ler - Her model için bir tablo oluşturur
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        /// <summary>
        /// Model konfigürasyonları - Tablo ilişkileri ve kuralları
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Tenant tablosu için unique constraint - Domain tekrarlanamaz
            modelBuilder.Entity<Tenant>()
                .HasIndex(t => t.Domain)
                .IsUnique();
                
            // BlogPost - Tenant ilişkisi (Cascade delete - Tenant silinirse yazıları da silinir)
            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.Tenant)
                .WithMany(t => t.BlogPosts)
                .HasForeignKey(bp => bp.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Category - Tenant ilişkisi
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Tenant)
                .WithMany(t => t.Categories)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // BlogPost - Category ilişkisi (Restrict - Kategori silinirse yazı silinmez)
            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(bp => bp.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}