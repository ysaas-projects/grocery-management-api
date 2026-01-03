using grocery_management.Data;
using grocery_management.Models;
using grocery_management.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace grocery_management.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseApiController
    {

        private readonly IJwtService _jwtService;
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        //private readonly IMillService _millService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
                    IJwtService jwtService,
                    IAuthService authService,
                    ApplicationDbContext context,
                    IConfiguration configuration,
                    //IMillService millService,
                    ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _authService = authService;
            _context = context;
            _config = configuration;
            //_millService = millService;
            _logger = logger;
        }


        // ✅ Get current logged-in user info
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var user = await _context.Users
                    //.Include(u => u.Firm)
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound("User not found");

                var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();

                return Ok(new
                {
                    user.UserId,
                    user.UserName,
                    user.Email,
                    user.MobileNumber,
                    user.LastLoginAt,
                    user.FirmId,
                    //FirmName = user.Firm?.FirmName,
                    //FirmCode = user.Firm?.FirmCode,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user data");
                return StatusCode(500, "Error retrieving user data");
            }
        }

        // ✅ Login - returns JWT access & refresh tokens
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var user = await _context.Users
                //.Include(u => u.Firm)
                .FirstOrDefaultAsync(u => u.UserName == request.Username);

            if (user == null)
                return BadRequest("Invalid username or password");

            // Account lockout settings
            var maxAttempts = _config.GetValue<int>("AccountLockout:MaxFailedAccessAttempts", 5);
            var lockoutMinutes = _config.GetValue<int>("AccountLockout:DefaultLockoutMinutes", 30);

            if (user.LockoutEnabled && user.LockoutEnd > DateTime.UtcNow)
                return BadRequest($"Account locked until {user.LockoutEnd:u}");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= maxAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    user.LockoutEnabled = true;
                }

                await _context.SaveChangesAsync();
                return BadRequest("Invalid username or password");
            }

            // Reset failed attempts
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            // Generate JWT tokens
            var (accessToken, jti) = _jwtService.GenerateAccessTokenWithJti(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save session
            var session = new UserSession
            {
                UserId = user.UserId,
                SessionId = Guid.NewGuid(),
                //AccessToken = jti,
                AccessToken = accessToken,     //  store actual JWT string
                Jti = jti,                     //  store unique token ID
                RefreshToken = BCrypt.Net.BCrypt.HashPassword(refreshToken),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpiryDays()),
                DeviceId = GenerateDeviceFingerprint(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            var activeStatus = false;

            //if (user?.FirmType == "Mill")
            //{
            //    var millId = user.FirmId ?? 0;
            //    var mill = await _context.Mills
            //        .Include(m => m.City)
            //        .Include(m => m.State)
            //        .Where(m => m.MillId == millId && !m.IsDeleted)
            //        .Select(m => new MillResponseDto
            //        {
            //            MillId = m.MillId,
            //            MillName = m.MillName,
            //            MillCode = m.MillCode,
            //            Address = m.Address,
            //            CityId = m.CityId,
            //            CityName = m.City!.CityName,
            //            StateId = m.StateId,
            //            StateName = m.State!.StateName,
            //            ContactPerson = m.ContactPerson,
            //            ContactNumber = m.ContactNumber,
            //            Email = m.Email,
            //            GstNumber = m.GstNumber,
            //            Country = m.Country ?? "India",
            //            Pincode = m.Pincode,
            //            IsActive = m.IsActive,
            //            CreatedAt = m.CreatedAt,
            //            UpdatedAt = m.UpdatedAt
            //        })
            //        .FirstOrDefaultAsync();

            //    //var mill = await _millService.GetMillsByIdAsync(user.FirmId ?? 0);
            //    activeStatus = mill.IsActive;
            //}
            //else if (user?.FirmType == "Company")
            //{
            //    // add here company code
            //}

            // Return JWT tokens in response body
            var response = new AuthResponse
            {
                UserId = user.UserId,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles,
                FirmId = user.FirmId,
                ActiveStatus = activeStatus,
                //FirmName = user.Firm?.FirmName,
                //FirmCode = user.Firm?.FirmCode,
                FirmType = user.FirmType,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpiryMinutes()),
                SessionId = session.SessionId,
                SessionExpiresAt = session.RefreshTokenExpiry,
                LastLoginAt = user.LastLoginAt
            };


            SetAuthCookies(accessToken, refreshToken);

            return Ok(response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked)
                .ToListAsync();

            foreach (var s in sessions)
            {
                s.IsRevoked = true;
                s.RevokedAt = DateTime.UtcNow;
                s.RevokedReason = "User logout";
            }

            await _context.SaveChangesAsync();

            //  Clear cookies
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out" });
        }


        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var accessToken = Request.Cookies["accessToken"];
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    return Unauthorized("Missing tokens");

                var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
                var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsRevoked);

                if (session == null || session.RefreshTokenExpiry < DateTime.UtcNow)
                    return Unauthorized("Invalid session");

                if (!BCrypt.Net.BCrypt.Verify(refreshToken, session.RefreshToken))
                    return Unauthorized("Invalid refresh token");

                var user = await _authService.GetUserById(userId);
                var roles = await _context.UserRoles
                    .Where(r => r.UserId == userId)
                    .Select(r => r.Role.RoleName)
                    .ToListAsync();

                var (newAccessToken, newJti) = _jwtService.GenerateAccessTokenWithJti(user, roles);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                session.RefreshToken = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
                session.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpiryDays());

                await _context.SaveChangesAsync();

                // ✅ SET COOKIES
                SetAuthCookies(newAccessToken, newRefreshToken);

                return Ok(new { success = true });
            }
            catch
            {
                return Unauthorized();
            }
        }


        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,              // JS cannot access
                Secure = true,                // HTTPS only
                SameSite = SameSiteMode.None, // Required for cross-domain frontend/backend
                Path = "/",                   // Cookie available site-wide
                IsEssential = true            // GDPR compliance
            };

            // Access Token (short-lived)
            cookieOptions.Expires = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpiryMinutes());
            Response.Cookies.Append("accessToken", accessToken, cookieOptions);

            // Refresh Token (long-lived)
            cookieOptions.Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpiryDays());
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }



        private string GenerateDeviceFingerprint()
        {
            var userAgent = Request.Headers.UserAgent.ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    Encoding.UTF8.GetBytes($"{userAgent}{ip}")));
        }



    }
}
