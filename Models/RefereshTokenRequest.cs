using Swashbuckle.AspNetCore.Annotations;

namespace grocery_management.Models
{
    [SwaggerSchema("Request for token refresh operation")]
    public class RefreshTokenRequest
    {
        [SwaggerSchema("Expired JWT access token", Nullable = true)]
        public string? AccessToken { get; set; }

        [SwaggerSchema("Valid refresh token", Nullable = true)]
        public string? RefreshToken { get; set; }
    }
}
