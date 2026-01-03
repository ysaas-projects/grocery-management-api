
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short RoleId { get; set; }

        [StringLength(50)]
        public string RoleName { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Navigation property for many-to-many relationship
    }

    public class CreateRoleDTO
    {
        public string RoleName { get; set; }

    }

    public class UpdateRoleDTO
    {

        public short RoleId { get; set; }


        public string RoleName { get; set; }

        public bool IsActive { get; set; } = true;
    }


    public class RoleResponseDto
    {
        public short RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
