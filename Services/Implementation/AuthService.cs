using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using grocery_management.Data;
using grocery_management.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using grocery_management.Services.Interface;

namespace grocery_management.Services.Implementation
{
	public class AuthService : IAuthService
	{
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;

		public AuthService(ApplicationDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<User> ValidateUser(string username, string password)
		{
			var user = await _context.Users
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.FirstOrDefaultAsync(u => u.UserName == username);

			if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
				return null;

			if (user.LockoutEnabled && user.LockoutEnd > DateTime.UtcNow)
				throw new Exception("Account is locked");

			return user;
		}

		public async Task<User> GetUserById(int userId)
		{
			return await _context.Users.FindAsync(userId);
		}

		public async Task RevokeRefreshToken(string refreshToken)
		{
			var sessions = await _context.UserSessions
				.Where(s => !s.IsRevoked)
				.ToListAsync();

			var session = sessions.FirstOrDefault(s => BCrypt.Net.BCrypt.Verify(refreshToken, s.RefreshToken));

			if (session != null)
			{
				session.IsRevoked = true;
				session.RevokedAt = DateTime.UtcNow;	
				await _context.SaveChangesAsync();
			}
		}

		public async Task UpdateUserLastLogin(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user != null)
			{
				user.LastLoginAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}

		public async Task<TokenPair?> RefreshTokenAsync(string refreshToken)
		{
			var session = await _context.UserSessions
				.Include(s => s.User)
				.ThenInclude(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.FirstOrDefaultAsync(s => !s.IsRevoked && s.RefreshTokenExpiry > DateTime.UtcNow);

			if (session == null || !BCrypt.Net.BCrypt.Verify(refreshToken, session.RefreshToken))
				return null;

			var user = session.User;
			if (user == null)
				return null;

			// Revoke old session
			session.IsRevoked = true;
			session.RevokedAt = DateTime.UtcNow;

			// Generate new tokens
			var (accessToken, jti) = GenerateJwtToken(user);
			var newRefreshToken = Guid.NewGuid().ToString();

			var refreshExpiry = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays"));

			var newSession = new UserSession
			{
				UserId = user.UserId,
				AccessToken = jti, // store JTI, not the full token
				RefreshToken = BCrypt.Net.BCrypt.HashPassword(newRefreshToken),
				RefreshTokenExpiry = refreshExpiry,
				CreatedAt = DateTime.UtcNow,
				LastRefreshedAt = DateTime.UtcNow
			};

			_context.UserSessions.Add(newSession);
			await _context.SaveChangesAsync();

			return new TokenPair
			{
				AccessToken = accessToken,
				RefreshToken = newRefreshToken,
			};
		}

		private (string Token, string Jti) GenerateJwtToken(User user)
		{
			var jwtConfig = _configuration.GetSection("Jwt");
			var jti = Guid.NewGuid().ToString();
			var sessionId = Guid.NewGuid().ToString();

			var claims = new List<Claim>
	{
		new Claim(JwtRegisteredClaimNames.Jti, jti),
		new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
		new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
		new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
		new Claim("firmId", user.FirmId?.ToString() ?? ""),
		new Claim("sessionId", sessionId)
	};

			foreach (var role in user.UserRoles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role.Role.RoleName));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["AccessTokenSecret"]!));
			var token = new JwtSecurityToken(
				issuer: jwtConfig["Issuer"],
				audience: jwtConfig["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(jwtConfig.GetValue<int>("AccessTokenExpirationMinutes")),
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
			);

			return (new JwtSecurityTokenHandler().WriteToken(token), jti);
		}


        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }


        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.SecurityStamp = Guid.NewGuid(); // Invalidate existing sessions/tokens
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
