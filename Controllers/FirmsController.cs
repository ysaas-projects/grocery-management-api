using grocery_management.Data;
using grocery_management.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace grocery_management.Controllers
{
    [Authorize(AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme + "," +
        CookieAuthenticationDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class FirmsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public FirmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================
        // GET ALL FIRMS → Super-Admin
        // ================================
        [HttpGet]
        [Authorize(Roles = "Super-Admin")]
        public async Task<IActionResult> GetFirms()
        {
            try
            {
                var firms = await _context.Firms
                    .Where(f => !f.IsDeleted)
                    .OrderBy(f => f.FirmName)
                    .Select(f => new
                    {
                        f.FirmId,
                        f.FirmName,
                        f.FirmCode,
                        f.Address,
                        f.ContactNumber,
                        f.ContactPerson,
                        f.GstNumber,
                        f.LogoImagePath,
                        f.IsActive
                    })
                    .ToListAsync();

                return ApiResponse(true, "Firms retrieved successfully", firms);
            }
            catch (Exception ex)
            {
                return ApiResponse(false, "Error retrieving firms", error: ex.Message);
            }
        }

        // ================================
        // GET FIRM BY ID → Firm-Admin + Super-Admin
        // ================================
        [HttpGet("{id}")]
        [Authorize(Roles = "Firm-Admin,Super-Admin")]
        public async Task<IActionResult> GetFirmById(int id)
        {
            // Firm-Admin → only own firm
            if (User.IsInRole("Firm-Admin"))
            {
                var firmIdClaim = User.FindFirst("firmId")?.Value;
                if (string.IsNullOrEmpty(firmIdClaim) || int.Parse(firmIdClaim) != id)
                    return Forbid();
            }

            var firm = await _context.Firms
                .Where(f => f.FirmId == id && !f.IsDeleted)
                .Select(f => new
                {
                    f.FirmId,
                    f.FirmName,
                    f.FirmCode,
                    f.Address,
                    f.ContactNumber,
                    f.ContactPerson,
                    f.GstNumber,
                    f.LogoImagePath,
                    f.IsActive
                })
                .FirstOrDefaultAsync();

            if (firm == null)
                return ApiResponse(false, "Firm not found", error: "NotFound");

            return ApiResponse(true, "Firm retrieved successfully", firm);
        }

        // ================================
        // CREATE FIRM → Super-Admin
        // ================================
        [HttpPost]
        [Authorize(Roles = "Super-Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateFirm([FromForm] FirmCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse(false, "Invalid data");

            dto.FirmCode = dto.FirmCode.Trim();

            bool exists = await _context.Firms.AnyAsync(f =>
                !f.IsDeleted &&
                (f.FirmName.ToLower() == dto.FirmName.ToLower()
                 || f.FirmCode == dto.FirmCode));

            if (exists)
                return ApiResponse(false, "Firm already exists", error: "Duplicate");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ================================
                // 1️⃣ SAVE LOGO (OPTIONAL)
                // ================================
                string? logoPath = null;

                if (dto.Logo != null && dto.Logo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName =
                        $"{Guid.NewGuid()}{Path.GetExtension(dto.Logo.FileName)}";

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Logo.CopyToAsync(stream);
                    }

                    var request = HttpContext.Request;
                    logoPath = $"{request.Scheme}://{request.Host}/uploads/{fileName}";
                }

                // ================================
                // 2️⃣ CREATE FIRM
                // ================================
                var firm = new Firm
                {
                    FirmName = dto.FirmName.Trim(),
                    FirmCode = dto.FirmCode,
                    Address = dto.Address,
                    ContactNumber = dto.ContactNumber,
                    ContactPerson = dto.ContactPerson,
                    GstNumber = dto.GstNumber,
                    LogoImagePath = logoPath,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Firms.Add(firm);
                await _context.SaveChangesAsync();
                var firmAdminRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == "Firm-Admin");

                if (firmAdminRole != null)
                {
                    var adminUser = new User
                    {
                        UserName = $"{firm.FirmCode}-admin".ToLower(),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@1234"),
                        FirmId = firm.FirmId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,   // ✅ ADD
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();

                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = adminUser.UserId,
                        RoleId = firmAdminRole.RoleId,

                        FirmId = firm.FirmId,
                        IsActive = true
                    });

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // ================================
                // 5️⃣ RESPONSE
                // ================================
                return ApiResponse(true, "Firm created successfully", new
                {
                    firm.FirmId,
                    firm.FirmName,
                    
                    firm.LogoImagePath
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ApiResponse(false, "Error creating firm", error: ex.Message);
            }
        }


        // ================================
        // UPDATE FIRM → Firm-Admin + Super-Admin
        // ================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Firm-Admin,Super-Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFirm(int id, [FromForm] FirmUpdateDto dto)
        {
            if (User.IsInRole("Firm-Admin"))
            {
                var firmIdClaim = User.FindFirst("firmId")?.Value;
                if (string.IsNullOrEmpty(firmIdClaim) || int.Parse(firmIdClaim) != id)
                    return Forbid();
            }

            var firm = await _context.Firms
                .FirstOrDefaultAsync(f => f.FirmId == id && !f.IsDeleted);

            if (firm == null)
                return ApiResponse(false, "Firm not found");

            firm.FirmName = dto.FirmName;
            firm.FirmCode = dto.FirmCode;
            firm.Address = dto.Address;
            firm.ContactNumber = dto.ContactNumber;
            firm.ContactPerson = dto.ContactPerson;
            firm.GstNumber = dto.GstNumber;
            firm.IsActive = dto.IsActive;
            firm.UpdatedAt = DateTime.UtcNow;

            // UPDATE LOGO (OPTIONAL)
            if (dto.Logo != null && dto.Logo.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads"
                );

                Directory.CreateDirectory(uploadsFolder);

                var fileName =
                    $"{Guid.NewGuid()}{Path.GetExtension(dto.Logo.FileName)}";

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Logo.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                firm.LogoImagePath =
                    $"{request.Scheme}://{request.Host}/uploads/{fileName}";
            }

            await _context.SaveChangesAsync();

            return ApiResponse(true, "Firm updated successfully");
        }

        // ================================
        // DELETE FIRM → Super-Admin
        // ================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Super-Admin")]
        public async Task<IActionResult> DeleteFirm(int id)
        {
            var firm = await _context.Firms
                .FirstOrDefaultAsync(f => f.FirmId == id && !f.IsDeleted);

            if (firm == null)
                return ApiResponse(false, "Firm not found", error: "NotFound");

            firm.IsDeleted = true;
            firm.IsActive = false;
            firm.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse(true, "Firm deleted successfully");
        }

        // ================================
        // GET LOGIN FIRM → Firm-Admin + Super-Admin
        // ================================
        [HttpGet("me")]
        [Authorize(Roles = "Firm-Admin,Super-Admin")]
        public async Task<IActionResult> GetMyFirm()
        {
            var firmIdClaim = User.FindFirst("firmId")?.Value;

            if (string.IsNullOrEmpty(firmIdClaim))
                return ApiResponse(true, "User has no firm assigned", null);

            int firmId = int.Parse(firmIdClaim);

            var firm = await _context.Firms
                .Where(f => f.FirmId == firmId && !f.IsDeleted)
                .Select(f => new
                {
                    f.FirmId,
                    f.FirmName,
                    f.FirmCode,
                    f.Address,
                    f.ContactNumber,
                    f.ContactPerson,
                    f.GstNumber,
                    f.LogoImagePath,
                    f.IsActive
                })
                .FirstOrDefaultAsync();

            if (firm == null)
                return ApiResponse(false, "Firm not found", error: "NotFound");

            return ApiResponse(true, "Login firm retrieved successfully", firm);
        }
    }
}
