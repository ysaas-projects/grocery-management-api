using System.Text.Json.Serialization;

namespace grocery_management.Models
{
    public class AuthResponse
    {
        // Core user information
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int? FirmId { get; set; }
        public Boolean ActiveStatus { get; set; } = false;

        public string FirmType { get; set; }
        public string FirmCode { get; set; }

        // Token information (for login/refresh)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string AccessToken { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string RefreshToken { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? AccessTokenExpiry { get; set; }

        // Session information (for auth check)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsAuthenticated { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? SessionId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? SessionCreatedAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DeviceInfo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string IPAddress { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastLoginAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? SessionExpiresAt { get; set; }
    }

    public class AuthCheckResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int? FirmId { get; set; }
        public string FirmName { get; set; }
        public string FirmCode { get; set; }
        public bool IsAuthenticated { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime? SessionCreatedAt { get; set; }
        public string DeviceInfo { get; set; }
        public string IPAddress { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? SessionExpiresAt { get; set; }


    }
}
