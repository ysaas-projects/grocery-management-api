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
    public class ProductStocksController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductStocksController> _logger;

        public ProductStocksController(ApplicationDbContext context, ILogger<ProductStocksController> logger)
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var data = await _context.ProductStocks
                    .Include(x => x.Product)
                    .Include(x => x.Firm)
                    .Where(x => x.FirmId == firmId)
                    .OrderByDescending(x => x.ProductStockId)
                    .Select(x => new ProductStockResponseDto
                    {
                        ProductStockId = x.ProductStockId,
                        FirmId = x.FirmId,
                        FirmName = x.Firm != null ? x.Firm.FirmName : null,
                        ProductId = x.ProductId,
                        ProductName = x.Product != null ? x.Product.ProductName : null,
                        Quantity = x.Quantity,
                        Remark = x.Remark,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "ProductStocks fetched successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll ProductStocks");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }


        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var stock = await _context.ProductStocks
                    .Include(x => x.Product)
                    .Include(x => x.Firm)
                    .Where(x => x.FirmId == firmId && x.ProductId == productId)
                    .Select(x => new ProductStockResponseDto
                    {
                        ProductStockId = x.ProductStockId,
                        FirmId = x.FirmId,
                        FirmName = x.Firm != null ? x.Firm.FirmName : null,
                        ProductId = x.ProductId,
                        ProductName = x.Product != null ? x.Product.ProductName : null,
                        Quantity = x.Quantity,
                        Remark = x.Remark,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                //if (stock == null)
                //    return ApiResponse(false, "Stock not found", statusCode: 404);

                return ApiResponse(true, "Product stock retrieved successfully", stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByProductId ProductStocks");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }


        [HttpGet("paginated")]
        public async Task<IActionResult> GetPaginated(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] int? productId = null)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var query = _context.ProductStocks
                    .Include(x => x.Product)
                    .Where(x => x.FirmId == firmId);

                if (productId.HasValue)
                    query = query.Where(x => x.ProductId == productId);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.ProductStockId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new ProductStockResponseDto
                    {
                        ProductStockId = x.ProductStockId,
                        FirmId = x.FirmId,
                        FirmName = x.Firm != null ? x.Firm.FirmName : null,
                        ProductId = x.ProductId,
                        ProductName = x.Product != null ? x.Product.ProductName : null,
                        Quantity = x.Quantity,
                        Remark = x.Remark,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "ProductStocks retrieved successfully", new
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
                _logger.LogError(ex, "Error in GetPaginated ProductStocks");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }
    }
}
