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
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ApplicationDbContext context,
            ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ================================
        // GET FirmId from JWT
        // ================================
        private int? GetFirmIdFromToken()
        {
            var firmIdStr = User.FindFirstValue("firmId");
            if (int.TryParse(firmIdStr, out var firmId))
                return firmId;

            return null;
        }

        // ================================
        // GET ALL CATEGORIES
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var categories = await _context.Categories
                    .Where(c => c.FirmId == firmId && !c.IsDeleted)
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new CategoryResponseDto
                    {
                        CategoryId = c.CategoryId,
                        FirmId = c.FirmId,
                        FirmName = c.Firm.FirmName,   // 👈 IMPORTANT

                        CategoryName = c.CategoryName,
                        ParentCategoryId = c.ParentCategoryId,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "Categories fetched successfully", categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllCategories");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // GET CATEGORY BY ID
        // ================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var category = await _context.Categories
                    .Where(c =>
                        c.CategoryId == id &&
                        c.FirmId == firmId &&
                        !c.IsDeleted)
                    .Select(c => new CategoryResponseDto
                    {
                        CategoryId = c.CategoryId,
                        FirmId = c.FirmId,
                        FirmName = c.Firm.FirmName,   // 👈 IMPORTANT

                        CategoryName = c.CategoryName,
                        ParentCategoryId = c.ParentCategoryId,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                return ApiResponse(true, "Category fetched successfully", category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCategoryById {CategoryId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // CREATE CATEGORY
        // ================================
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(false, "Validation failed",
                    errors: ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList(),
                    statusCode: 400);
            }

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                bool exists = await _context.Categories.AnyAsync(c =>
                    c.FirmId == firmId &&
                    !c.IsDeleted &&
                    c.CategoryName.ToLower() == dto.CategoryName.ToLower());

                if (exists)
                    return ApiResponse(false, "Category already exists");

                var category = new Category
                {
                    FirmId = firmId.Value,
                    CategoryName = dto.CategoryName,
                    ParentCategoryId = dto.ParentCategoryId,
                    IsActive = dto.IsActive,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return ApiResponse(true, "Category created successfully", new
                {
                    category.CategoryId,
                    category.CategoryName
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCategory");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // UPDATE CATEGORY
        // ================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(false, "Validation failed",
                    errors: ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList(),
                    statusCode: 400);
            }

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                if (id != dto.CategoryId)
                    return ApiResponse(false, "CategoryId mismatch");

                var category = await _context.Categories
                    .Where(c =>
                        c.CategoryId == id &&
                        c.FirmId == firmId &&
                        !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (category == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                category.CategoryName = dto.CategoryName;
                category.ParentCategoryId = dto.ParentCategoryId;
                category.IsActive = dto.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCategory {CategoryId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // DELETE CATEGORY (SOFT DELETE)
        // ================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var category = await _context.Categories
                    .Where(c =>
                        c.CategoryId == id &&
                        c.FirmId == firmId &&
                        !c.IsDeleted)
                    .FirstOrDefaultAsync();

                if (category == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                category.IsDeleted = true;
                category.IsActive = false;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCategory {CategoryId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }
    }
}
