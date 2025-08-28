using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantBlog.Models
{
    /// <summary>
    /// Blog yazısı modeli - Her yazı bir tenant'a ait
    /// </summary>
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TenantId { get; set; } // Hangi tenant'a ait olduğunu belirler
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } // Yazı başlığı
        
        [StringLength(500)]
        public string? Summary { get; set; } // Kısa özet
        
        [Required]
        public string Content { get; set; } // Yazı içeriği (HTML olabilir)
        
        [StringLength(100)]
        public string Author { get; set; } = "Admin"; // Yazar adı
        
        public DateTime PublishedDate { get; set; } = DateTime.Now; // Yayın tarihi
        
        public DateTime? UpdatedDate { get; set; } // Güncellenme tarihi
        
        public bool IsPublished { get; set; } = true; // Yayında mı?
        
        public int CategoryId { get; set; } // Kategori ID'si
        
        // Navigation Properties
        [ForeignKey("TenantId")] // Foreign key ilişkisi
        public Tenant Tenant { get; set; } = null!;
        
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
    }
}