using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserRoleId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public short RoleId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int FirmId { get; set; }
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        [ForeignKey("FirmId")]

        public virtual Firm Firm { get; set; }

    }

    public class CreateUserRoleDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public short RoleId { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateUserRoleDTO
    {
        [Required]
        public int UserRoleId { get; set; }

        public int? UserId { get; set; }
        public short? RoleId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserRoleDTO
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public short RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
