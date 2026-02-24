using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grocery_management.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        //[ForeignKey("Firm")]
        //[Required(ErrorMessage = "Firm selection is required")]
        public int? FirmId { get; set; }

        public string? FirmType { get; set; }



        [StringLength(30, MinimumLength = 3, ErrorMessage = "Minimum 3 characters are required")]
        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression(@"^[a-zA-Z0-9@_\-\.]+$", ErrorMessage = "Username can only contain letters, numbers, and @ _ - .")]
        public string UserName { get; set; }

        [StringLength(255, MinimumLength = 3, ErrorMessage = "Minimum 3 characters are required")]
        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }

        [StringLength(255)]
        //[Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [StringLength(20)]
        //[Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10-digit mobile number")]
        public string? MobileNumber { get; set; }
        public bool MobileNumberConfirmed { get; set; } = false;

        public byte AccessFailedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public Guid SecurityStamp { get; set; } = Guid.NewGuid();

        public DateTime? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; } = true;

        public bool TwoFactorEnabled { get; set; } = false;


        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
       
        [ForeignKey(nameof(FirmId))]
        public Firm? Firm { get; set; }
    }

    // DTOs for CRUD operations 

    public class DTOUserCreate
    {
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Minimum 3 characters are required")]
        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression(@"^[a-zA-Z0-9@_\-\.]+$", ErrorMessage = "Username can only contain letters, numbers, and @ _ - .")]
        public string UserName { get; set; }

        [StringLength(255, MinimumLength = 3, ErrorMessage = "Minimum 3 characters are required")]
        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }

        [StringLength(255)]
        //[Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [StringLength(20)]
        //[Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10-digit mobile number")]
        public string? MobileNumber { get; set; }

        //[Required(ErrorMessage = "Firm selection is required")]
        public int? FirmId { get; set; }

        public string? FirmType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DTOUserUpdate
    {
        public int UserId { get; set; }


        [StringLength(30, MinimumLength = 3, ErrorMessage = "Minimum 3 characters are required")]
        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression(@"^[a-zA-Z0-9@_\-\.]+$", ErrorMessage = "Username can only contain letters, numbers, and @ _ - .")]
        public string UserName { get; set; }

        [StringLength(255)]
        //[Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [StringLength(20)]
        //[Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10-digit mobile number")]
        public string? MobileNumber { get; set; }

        //[Required(ErrorMessage = "Firm selection is required")]
        public int? FirmId { get; set; }

        public string? FirmType { get; set; }

        public bool IsActive { get; set; }
    }
}
