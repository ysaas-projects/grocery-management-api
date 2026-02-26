using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{

    [Table("ProductBatches")]
    public class ProductBatch
    {
        [Key]
        public int BatchId { get; set; }

        public int? FirmId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [StringLength(50)]
        public string? BatchNumber { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MRP { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal RemainingQty { get; set; }

        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(FirmId))]
        public Firm? Firm { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;
    }


    public class ProductBatchCreateDto
    {
        [Required]
        public int ProductId { get; set; }


        [StringLength(50)]
        public string? BatchNumber { get; set; }

        public decimal MRP { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public decimal Quantity { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }

    
    public class ProductBatchUpdateDto
    {
        [Required]
        public int BatchId { get; set; }


        [StringLength(50)]
        public string? BatchNumber { get; set; }

        public decimal MRP { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }


    public class SellProductDto
    {
        public int ProductId { get; set; }

        public decimal SellQuantity { get; set; }
    }

    public class ProductBatchResponseDto
    {
        public int BatchId { get; set; }

        public int? FirmId { get; set; }

        public string? FirmName { get; set; }

        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? BatchNumber { get; set; }

        public decimal MRP { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public decimal Quantity { get; set; }

        public decimal RemainingQty { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}