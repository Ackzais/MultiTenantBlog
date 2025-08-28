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
        
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty; // Varsayılan değer
        
        [StringLength(500, ErrorMessage = "Özet en fazla 500 karakter olabilir")]
        public string? Summary { get; set; } // Nullable
        
        [Required(ErrorMessage = "İçerik gereklidir")]
        public string Content { get; set; } = string.Empty; // Varsayılan değer
        
        [StringLength(100, ErrorMessage = "Yazar adı en fazla 100 karakter olabilir")]
        public string Author { get; set; } = "Admin"; // Varsayılan değer
        
        public DateTime PublishedDate { get; set; } = DateTime.Now; // Varsayılan değer
        
        public DateTime? UpdatedDate { get; set; } // Nullable
        
        public bool IsPublished { get; set; } = true; // Varsayılan değer
        
        [Required(ErrorMessage = "Kategori seçilmelidir")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçin")]
        public int CategoryId { get; set; } // Kategori ID'si
        
        // Navigation Properties
        [ForeignKey("TenantId")] // Foreign key ilişkisi
        public Tenant Tenant { get; set; } = null!;
        
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
    }
}