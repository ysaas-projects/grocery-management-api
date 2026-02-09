using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        [Required]
        public int FirmId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // 🔗 Navigation
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(FirmId))]
        public Firm Firm { get; set; } = null!;
    }

    // =========================
    // DTOs
    // =========================

    public class ProductImageCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public int SortOrder { get; set; } = 0;
        [Required]
        public IFormFile Image { get; set; } = null!;
    }

    public class ProductImageResponseDto
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
