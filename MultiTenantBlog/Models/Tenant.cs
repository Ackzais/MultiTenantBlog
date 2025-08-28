using System.ComponentModel.DataAnnotations;

namespace MultiTenantBlog.Models
{
    /// <summary>
    /// Tenant (Kiracı) modeli - Her blog sitesi için bir tenant kaydı
    /// </summary>
    public class Tenant
    {
        [Key] // Primary key olarak işaretler
        public int Id { get; set; }
        
        [Required] // Bu alan zorunlu
        [StringLength(100)] // Maksimum 100 karakter
        public string Name { get; set; } // Blog sitesi adı (örn: "Tech Blog")
        
        [Required]
        [StringLength(200)]
        public string Domain { get; set; } // Domain adı (örn: "techblog.com")
        
        [StringLength(200)]
        public string? Subdomain { get; set; } // Subdomain (örn: "tech.myblog.com")
        
        [StringLength(500)]
        public string? Description { get; set; } // Blog açıklaması
        
        [StringLength(50)]
        public string Theme { get; set; } = "Default"; // Tema adı
        
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Oluşturulma tarihi
        
        public bool IsActive { get; set; } = true; // Aktif mi?
        
        // Navigation Properties - İlişkili tablolara erişim
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}