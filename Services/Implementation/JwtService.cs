using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using grocery_management.Data;
using grocery_management.Models;
using grocery_management.Services.Interface;

namespace grocery_management.Services.Implementation
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public JwtService(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }
        private string GetConfigValue(string key)
        {
            var value = _config[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Missing JWT configuration for key: {key}");
            return value;
        }

        public (string Token, string Jti) GenerateAccessTokenWithJti(User user, List<string> roles)
        {
            var jti = Guid.NewGuid().ToString();
            // Generate a new session ID
            var sessionId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),		// additional changes for userId claim
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, GetConfigValue("Jwt:Issuer")),
                new Claim(JwtRegisteredClaimNames.Aud, GetConfigValue("Jwt:Audience")),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

				// Add session ID claim
				new Claim("sessionId", sessionId),  // This is the key addition
				// Add FirmId
				new Claim("firmId", user.FirmId?.ToString() ?? ""),

				new Claim("firmType", user.FirmType ?? "")

			};

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));      // additional changes for userId claim

            var key = new SymmetricSecurityKey(
                  //Encoding.UTF8.GetBytes(_config["Jwt:AccessTokenSecret"]));
                  Encoding.UTF8.GetBytes(GetConfigValue("Jwt:AccessTokenSecret")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                //expires: DateTime.Now.AddMinutes(GetAccessTokenExpiryMinutes()),
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), jti);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            // Ensure URL-safe Base64 encoding
            return Convert.ToBase64String(randomNumber)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:AccessTokenSecret"])),
                ValidateLifetime = false, // Still false for expired tokens
                ClockSkew = TimeSpan.FromMinutes(1), // Add this to match main config
                NameClaimType = "sessionId", // Add this to properly map the sessionId claim
                RoleClaimType = "role"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        }

        public int GetRefreshTokenExpiryDays() =>
            _config.GetValue<int>("Jwt:RefreshTokenExpirationDays");

        public int GetAccessTokenExpiryMinutes() =>
            _config.GetValue<int>("Jwt:AccessTokenExpirationMinutes");
    }
}
