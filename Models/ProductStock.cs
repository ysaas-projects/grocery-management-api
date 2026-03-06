using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{

    [Table("ProductStocks")]
    public class ProductStock
    {
        [Key]
        public int ProductStockId { get; set; }

        public int? FirmId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public string Remark { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }


        // Navigation Properties

        public virtual Firm? Firm { get; set; }

        public virtual Product? Product { get; set; }
    }

    public class ProductStockResponseDto
    {
        public int ProductStockId { get; set; }
        public int? FirmId { get; set; }
        public string? FirmName { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
