
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    public class ProductBatches
    {
        [Key]
        public long BatchId { get; set; }

        [Required]
        public int FirmId { get; set; }

        [Required]

        public int ProductId { get; set; }

        [Required]

        public string? BatchNumber { get; set; }

        [Required]
        public decimal MRP { get; set; }

        [Required]
        public decimal CostPrice { get; set; }

        [Required]
        public decimal SalePrice { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal RemainingQty { get; set; }

        [Required]
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }





    }
}
