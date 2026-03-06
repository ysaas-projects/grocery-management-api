using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace grocery_management.Models
{
    [Table("StockTransactions")]
   
    public class StockTransaction 
    {
        [Key]
        public int TransactionId { get; set; }

        public int? FirmId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? BatchId { get; set; }

        [Required]
        [StringLength(20)]
        public string TransactionType { get; set; } = null!;
        // SALE / PURCHASE / RETURN / ADJUSTMENT
        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        public bool IsIncrease { get; set; }

        public bool IsDecrease { get; set; }

        public int? ReferenceId { get; set; }

        [StringLength(20)]
        public string? ReferenceType { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }


        [ForeignKey(nameof(FirmId))]
        public Firm? Firm { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(BatchId))]
        public ProductBatch? ProductBatch { get; set; }
    }


   
    public class StockTransactionCreateDto
    {
        [Required]
        public int ProductId { get; set; }

        public int? BatchId { get; set; }

        [Required]
        public string TransactionType { get; set; } = null!;

        public decimal Quantity { get; set; }

        public bool IsIncrease { get; set; }

        public int? ReferenceId { get; set; }

        public string? ReferenceType { get; set; }

        public string? Notes { get; set; }
    }
   
    public class StockTransactionResponseDto
    {
        public long TransactionId { get; set; }

        public int? FirmId { get; set; }
        public string? FirmName { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        public int? BatchId { get; set; }
        public string? BatchNumber { get; set; }

        public string TransactionType { get; set; } = null!;

        public decimal Quantity { get; set; }

        public bool IsIncrease { get; set; }

        public int? ReferenceId { get; set; }

        public string? ReferenceType { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}