using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace grocery_management.Models
{
    public class AuthRequest
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [DefaultValue("admin")]

        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DefaultValue("admin123")]
        public string Password { get; set; }
    }

    public class TokenPair
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }


    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
