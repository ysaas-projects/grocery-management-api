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
    public class ProductsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ApplicationDbContext context,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        private int? GetFirmIdFromToken()
        {
            var firmIdStr = User.FindFirstValue("firmId");
            if (int.TryParse(firmIdStr, out var firmId))
                return firmId;

            return null;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var products = await _context.Products
                    .Include(p => p.Firm)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Where(p => p.FirmId == firmId && !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .Select(p => new ProductResponseDto
                    {
                        ProductId = p.ProductId,
                        FirmId = p.FirmId,
                        FirmName = p.Firm.FirmName,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.CategoryName,
                        ProductName = p.ProductName,
                        Barcode = p.Barcode,
                        Unit = p.Unit,
                        IsLooseItem = p.IsLooseItem,
                        MRP = p.MRP,
                        SalePrice = p.SalePrice,
                        GSTPercent = p.GSTPercent,
                        LowStockAlert = p.LowStockAlert,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                         PrimaryImageUrl = p.ProductImages
        .Where(img => !img.IsDeleted && img.IsPrimary)
        .OrderBy(img => img.SortOrder)
        .Select(img => img.ImageUrl)
        .FirstOrDefault()
                    })
                    .ToListAsync();

                return ApiResponse(true, "Products fetched successfully", products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProducts");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        
        [HttpGet("paginated")]
        public async Task<IActionResult> GetProductsPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? categoryId = null)
        {
            try
            {
                if (pageNumber < 1)
                    return ApiResponse(false, "Page number must be >= 1");

                if (pageSize < 1 || pageSize > 100)
                    return ApiResponse(false, "Page size must be 1–100");

                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var query = _context.Products
                    .Include(p => p.Firm)
                    .Include(p => p.Category)
                    .Where(p => p.FirmId == firmId && !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(p =>
                        p.ProductName.ToLower().Contains(search) ||
                        (p.Barcode != null && p.Barcode.ToLower().Contains(search)));
                }

                if (isActive.HasValue)
                    query = query.Where(p => p.IsActive == isActive.Value);

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                var totalCount = await query.CountAsync();
                var skip = (pageNumber - 1) * pageSize;

                var items = await query
                    .OrderBy(p => p.ProductName)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(p => new ProductResponseDto
                    {
                        ProductId = p.ProductId,
                        FirmId = p.FirmId,
                        FirmName = p.Firm.FirmName,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.CategoryName,
                        ProductName = p.ProductName,
                        Barcode = p.Barcode,
                        Unit = p.Unit,
                        IsLooseItem = p.IsLooseItem,
                        MRP = p.MRP,
                        SalePrice = p.SalePrice,
                        GSTPercent = p.GSTPercent,
                        LowStockAlert = p.LowStockAlert,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "Products retrieved successfully", new
                {
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    Items = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductsPaginated");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var product = await _context.Products
                    .Include(p => p.Firm)
                    .Include(p => p.Category)
                    .Where(p =>
                        p.ProductId == id &&
                        p.FirmId == firmId &&
                        !p.IsDeleted)
                    .Select(p => new ProductResponseDto
                    {
                        ProductId = p.ProductId,
                        FirmId = p.FirmId,
                        FirmName = p.Firm.FirmName,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.CategoryName,
                        ProductName = p.ProductName,
                        Barcode = p.Barcode,
                        Unit = p.Unit,
                        IsLooseItem = p.IsLooseItem,
                        MRP = p.MRP,
                        SalePrice = p.SalePrice,
                        GSTPercent = p.GSTPercent,
                        LowStockAlert = p.LowStockAlert,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                return ApiResponse(true, "Product fetched successfully", product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductById {ProductId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        // ================================
        // CREATE PRODUCT
        // ================================
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(false, "Validation failed",
                    errors: ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList(),
                    statusCode: 400);
            }

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                
                if (!string.IsNullOrWhiteSpace(dto.Barcode))
                {
                    bool barcodeExists = await _context.Products.AnyAsync(p =>
                        p.FirmId == firmId &&
                        !p.IsDeleted &&
                        p.Barcode == dto.Barcode);

                    if (barcodeExists)
                        return ApiResponse(false, "Barcode already exists");
                }

                // 💰 Price validation
                if (dto.SalePrice > dto.MRP)
                    return ApiResponse(false, "SalePrice cannot be greater than MRP");

                var product = new Product
                {
                    FirmId = firmId.Value,
                    CategoryId = dto.CategoryId,
                    ProductName = dto.ProductName,
                    Barcode = dto.Barcode,
                    Unit = dto.Unit,
                    IsLooseItem = dto.IsLooseItem,
                    MRP = dto.MRP,
                    SalePrice = dto.SalePrice,
                    GSTPercent = dto.GSTPercent,
                    LowStockAlert = dto.LowStockAlert,
                    IsActive = dto.IsActive,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return ApiResponse(true, "Product created successfully", new
                {
                    product.ProductId,
                    product.ProductName
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateProduct");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(false, "Validation failed",
                    errors: ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList(),
                    statusCode: 400);
            }

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                if (id != dto.ProductId)
                    return ApiResponse(false, "ProductId mismatch");

                var product = await _context.Products
                    .Where(p =>
                        p.ProductId == id &&
                        p.FirmId == firmId &&
                        !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (product == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                product.CategoryId = dto.CategoryId;
                product.ProductName = dto.ProductName;
                product.Barcode = dto.Barcode;
                product.Unit = dto.Unit;
                product.IsLooseItem = dto.IsLooseItem;
                product.MRP = dto.MRP;
                product.SalePrice = dto.SalePrice;
                product.GSTPercent = dto.GSTPercent;
                product.LowStockAlert = dto.LowStockAlert;
                product.IsActive = dto.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProduct {ProductId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Invalid firm access!",
                        error: "Unauthorized", statusCode: 401);

                var product = await _context.Products
                    .Where(p =>
                        p.ProductId == id &&
                        p.FirmId == firmId &&
                        !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (product == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                product.IsDeleted = true;
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProduct {ProductId}", id);
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }
    }
}
