using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int FirmId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation (optional)
        [ForeignKey(nameof(ParentCategoryId))]
        public Category? ParentCategory { get; set; }
        // 🔗 Firm navigation
        [ForeignKey(nameof(FirmId))]
        public Firm Firm { get; set; } = null!;
        public ICollection<Category> Children { get; set; } = new List<Category>();
    }

    public class CategoryCreateDto
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        public bool IsActive { get; set; } = true;
    }
    public class CategoryUpdateDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        public bool IsActive { get; set; }
    }
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int FirmId { get; set; }
        public string FirmName { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ProductCount { get; set; }
    }
}
