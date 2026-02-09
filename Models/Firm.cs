using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    [Table("Firms")]
    public class Firm
    {
        [Key]
        public int FirmId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirmName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirmCode { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(30)]
        public string? GstNumber { get; set; }

        [StringLength(500)]
        public string? LogoImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    public class FirmCreateDto
    {
        public string FirmName { get; set; } = string.Empty;
        public string FirmCode { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? GstNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public IFormFile? Logo { get; set; }
    }
    public class FirmUpdateDto
    {
        public string FirmName { get; set; } = string.Empty;
        public string FirmCode { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? GstNumber { get; set; }

        public bool IsActive { get; set; }
        public IFormFile? Logo { get; set; }
    }
}
