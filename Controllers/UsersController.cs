using grocery_management.Data;
using grocery_management.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace grocery_management.Controllers
{
    [Authorize(AuthenticationSchemes =
        CookieAuthenticationDefaults.AuthenticationScheme + "," +
        JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // GET ALL USERS
        // Super-Admin → all
        // Firm-Admin → own firm users
        // ================================
        [HttpGet]
        [Authorize(Roles = "Super-Admin,Firm-Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            bool isSuperAdmin = User.IsInRole("Super-Admin");

            int? firmId = null;
            if (!isSuperAdmin)
            {
                var firmIdStr = User.FindFirstValue("firmId");
                if (!int.TryParse(firmIdStr, out var parsedFirmId))
                    return ApiResponse(false, "Invalid firm access",
                        error: "Unauthorized", statusCode: 401);

                firmId = parsedFirmId;
            }

            var query = _context.Users
                .Include(u => u.Firm)
                .Where(u => !u.IsDeleted);

            if (!isSuperAdmin)
                query = query.Where(u => u.FirmId == firmId);

            var users = await query
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.MobileNumber,
                    u.FirmId,
                    FirmName = u.Firm != null ? u.Firm.FirmName : null,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .ToListAsync();

            return ApiResponse(true, "Users fetched successfully", users);
        }

        // ================================
        // GET USER BY ID
        // Self OR Admin
        // ================================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var currentUserId =
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (currentUserId != userId &&
                !User.IsInRole("Super-Admin") &&
                !User.IsInRole("Firm-Admin"))
                return Forbid();

            var user = await _context.Users
                .Include(u => u.Firm)
                .Where(u => u.UserId == userId && !u.IsDeleted)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.MobileNumber,
                    u.FirmId,
                    FirmName = u.Firm != null ? u.Firm.FirmName : null,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return ApiResponse(false, "User not found", statusCode: 404);

            return ApiResponse(true, "User fetched successfully", user);
        }

        // ================================
        // CHANGE PASSWORD
        // Self OR Super-Admin
        // ================================
        [HttpPost("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(
            int userId,
            [FromBody] ChangePasswordRequest request)
        {
            var currentUserId =
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (currentUserId != userId && !User.IsInRole("Super-Admin"))
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted)
                return ApiResponse(false, "User not found", statusCode: 404);

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest("Current password is incorrect");

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.SecurityStamp = Guid.NewGuid();
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse(true, "Password changed successfully");
        }

        // ================================
        // RESET PASSWORD
        // Super-Admin ONLY
        // ================================
        [HttpPost("{userId}/reset-password")]
        [Authorize(Roles = "Super-Admin")]
        public async Task<IActionResult> ResetPassword(
            int userId,
            [FromBody] string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted)
                return ApiResponse(false, "User not found", statusCode: 404);

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(newPassword);

            user.SecurityStamp = Guid.NewGuid();
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse(true, "Password reset successfully");
        }

        // ================================
        // TOGGLE USER STATUS
        // Super-Admin / Firm-Admin
        // ================================
        [HttpPost("{userId}/toggle-status")]
        [Authorize(Roles = "Super-Admin,Firm-Admin")]
        public async Task<IActionResult> ToggleUserStatus(
            int userId,
            [FromBody] bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted)
                return ApiResponse(false, "User not found", statusCode: 404);

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse(true, "User status updated successfully");
        }

        // ================================
        // DELETE USER (SOFT DELETE)
        // Super-Admin ONLY
        // ================================
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Super-Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted)
                return ApiResponse(false, "User not found", statusCode: 404);

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse(true, "User deleted successfully");
        }
    }
}
