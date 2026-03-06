using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace grocery_management.Models
{
    [Table("StockAdjustments")]    
    public class StockAdjustment
    {
        [Key]
        public int AdjustmentId { get; set; }

        public int? FirmId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? BatchId { get; set; }

        [StringLength(20)]
        public string? AdjustmentType { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
       

        [ForeignKey(nameof(FirmId))]
        public Firm? Firm { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(BatchId))]
        public ProductBatch? Batch { get; set; }
    }


    public class StockAdjustmentCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        public string? AdjustmentType { get; set; }

        public decimal Quantity { get; set; }

        public string? Reason { get; set; }
    }


    //public class StockAdjustmentUpdateDto
    //{
    //    [Required]
    //    public int AdjustmentId { get; set; }

    //    public string? AdjustmentType { get; set; }

    //    public decimal Quantity { get; set; }

    //    public string? Reason { get; set; }
    //}


    
    public class StockAdjustmentResponseDto
    {
        public int AdjustmentId { get; set; }

        public int? FirmId { get; set; }
        public string? FirmName { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public int? BatchId { get; set; }
        public string? BatchNumber { get; set; }

        public string? AdjustmentType { get; set; }

        public decimal Quantity { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}