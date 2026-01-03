using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    public class UserSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SessionId { get; set; } = Guid.NewGuid();

        [StringLength(255)]
        public string? Jti { get; set; } // Unique token ID for JWT validation


        [Required]
        public int UserId { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? AccessToken { get; set; }

        [Required]
        [StringLength(255)]
        public string RefreshToken { get; set; }

        [Required]
        public DateTime RefreshTokenExpiry { get; set; }

        [StringLength(255)]
        public string? DeviceId { get; set; }  // This matches your DeviceInfo in the controller

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(512)]
        public string? UserAgent { get; set; }  // This will store DeviceInfo from headers

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastRefreshedAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        [StringLength(100)]
        public string? RevokedReason { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

}
