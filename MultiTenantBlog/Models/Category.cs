using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiTenantBlog.Models
{
    /// <summary>
    /// Kategori modeli - Her kategori bir tenant'a ait
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TenantId { get; set; } // Hangi tenant'a ait
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Kategori adı (örn: "Teknoloji")
        
        [StringLength(300)]
        public string? Description { get; set; } // Kategori açıklaması
        
        [StringLength(50)]
        public string Color { get; set; } = "#007bff"; // Kategori rengi
        
        // Navigation Properties
        [ForeignKey("TenantId")]
        public Tenant Tenant { get; set; } = null!;
        
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}