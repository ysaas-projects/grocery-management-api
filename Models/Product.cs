using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int FirmId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Barcode { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;   // kg, gm, pcs, ltr

        public bool IsLooseItem { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MRP { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal GSTPercent { get; set; } = 0;

        public int? LowStockAlert { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // 🔗 Navigation (FK commented in DB, allowed in model)
        [ForeignKey(nameof(FirmId))]
        public Firm Firm { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
    public class ProductCreateDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        public bool IsLooseItem { get; set; } = false;

        [Required]
        public decimal MRP { get; set; }

        [Required]
        public decimal SalePrice { get; set; }

        public decimal GSTPercent { get; set; } = 0;

        public int? LowStockAlert { get; set; }

        public bool IsActive { get; set; } = true;
    }
    public class ProductUpdateDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        public bool IsLooseItem { get; set; }

        [Required]
        public decimal MRP { get; set; }

        [Required]
        public decimal SalePrice { get; set; }

        public decimal GSTPercent { get; set; }

        public int? LowStockAlert { get; set; }

        public bool IsActive { get; set; }
    }

    public class ProductResponseDto
    {
        public int ProductId { get; set; }

        public int FirmId { get; set; }
        public string FirmName { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;
        public string? Barcode { get; set; }

        public string Unit { get; set; } = string.Empty;
        public bool IsLooseItem { get; set; }

        public decimal MRP { get; set; }
        public decimal SalePrice { get; set; }
        public decimal GSTPercent { get; set; }

        public int? LowStockAlert { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }
}
