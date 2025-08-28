using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantBlog.Models
{
    /// <summary>
    /// Blog yazısı modeli - DÜZELTME: Validation attributes düzeltildi
    /// </summary>
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }
        
        // TenantId required olmaktan çıkarıldı (controller'da manuel atanıyor)
        public int TenantId { get; set; }
        
        [Required(ErrorMessage = "Yazı başlığı zorunludur.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Özet en fazla 500 karakter olabilir.")]
        public string? Summary { get; set; }
        
        [Required(ErrorMessage = "Yazı içeriği zorunludur.")]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Yazar adı en fazla 100 karakter olabilir.")]
        public string Author { get; set; } = "Admin";
        
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
        
        public bool IsPublished { get; set; } = true;
        
        // CategoryId required olmaktan çıkarıldı (controller'da kontrol ediliyor)
        public int CategoryId { get; set; }
        
        // Navigation Properties
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;
        
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
    }
}