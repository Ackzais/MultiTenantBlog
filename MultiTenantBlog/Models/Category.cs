using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantBlog.Models
{
    /// <summary>
    /// Kategori modeli - DÜZELTME: Validation attributes düzeltildi
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        // TenantId required olmaktan çıkarıldı (controller'da manuel atanıyor)
        public int TenantId { get; set; }
        
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
        public string? Description { get; set; }
        
        [StringLength(50, ErrorMessage = "Renk kodu en fazla 50 karakter olabilir.")]
        public string Color { get; set; } = "#007bff";
        
        // Navigation Properties
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;
        
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}