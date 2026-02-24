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
    public class ProductImagesController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductImagesController> _logger;

        public ProductImagesController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<ProductImagesController> logger)
        {
            _context = context;
            _env = env;
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
        // GET IMAGES BY PRODUCT
        // ================================
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetImagesByProduct(int productId)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access",
                        error: "Unauthorized", statusCode: 401);

                var images = await _context.ProductImages
                    .Where(i =>
                        i.ProductId == productId &&
                        i.FirmId == firmId &&
                        !i.IsDeleted)
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => new ProductImageResponseDto
                    {
                        ProductImageId = i.ProductImageId,
                        ProductId = i.ProductId,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "Product images fetched successfully", images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetImagesByProduct");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // UPLOAD PRODUCT IMAGE
        // ================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductImage(
            [FromForm] ProductImageCreateDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return ApiResponse(false, "Image is required");

            var firmId = GetFirmIdFromToken();
            if (firmId == null)
                return ApiResponse(false, "Invalid firm access",
                    error: "Unauthorized", statusCode: 401);

            var productExists = await _context.Products.AnyAsync(p =>
                p.ProductId == dto.ProductId &&
                p.FirmId == firmId &&
                !p.IsDeleted);

            if (!productExists)
                return ApiResponse(false, "Invalid product");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 📂 Ensure folder exists
                var uploadsPath = Path.Combine(_env.WebRootPath, "products");
                Directory.CreateDirectory(uploadsPath);

                // 📸 Save image
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var imageUrl = $"{request.Scheme}://{request.Host}/products/{fileName}";

                // 🔁 If primary → unset old primary
                if (dto.IsPrimary)
                {
                    var existingPrimary = await _context.ProductImages
                        .Where(i =>
                            i.ProductId == dto.ProductId &&
                            i.FirmId == firmId &&
                            i.IsPrimary &&
                            !i.IsDeleted)
                        .ToListAsync();

                    foreach (var img in existingPrimary)
                    {
                        img.IsPrimary = false;
                        img.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var productImage = new ProductImage
                {
                    FirmId = firmId.Value,
                    ProductId = dto.ProductId,
                    ImageUrl = imageUrl,
                    IsPrimary = dto.IsPrimary,
                    SortOrder = dto.SortOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ApiResponse(true, "Image uploaded successfully", new
                {
                    productImage.ProductImageId,
                    productImage.ImageUrl,
                    productImage.IsPrimary
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error uploading product image");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // DELETE IMAGE (SOFT DELETE)
        // ================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access",
                        error: "Unauthorized", statusCode: 401);

                var image = await _context.ProductImages
                    .FirstOrDefaultAsync(i =>
                        i.ProductImageId == id &&
                        i.FirmId == firmId &&
                        !i.IsDeleted);

                if (image == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                image.IsDeleted = true;
                image.IsPrimary = false;
                image.DeletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Image deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteImage {ImageId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }
    }
}
