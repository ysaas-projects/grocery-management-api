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
    public class ProductBatchesController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductBatchesController> _logger;

        public ProductBatchesController(
            ApplicationDbContext context,
            ILogger<ProductBatchesController> logger)
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
                    return ApiResponse(false, "Invalid firm access!", error: "Unauthorized", statusCode: 401);

                var batches = await _context.ProductBatches
                    .Include(b => b.Product)
                    .Include(b => b.Firm)
                    .Where(b => b.FirmId == firmId && !b.IsDeleted)
                    .OrderByDescending(b => b.BatchId)
                    .Select(b => new ProductBatchResponseDto
                    {
                        BatchId = b.BatchId,
                        FirmId = b.FirmId,
                        FirmName = b.Firm != null ? b.Firm.FirmName : null,
                        ProductId = b.ProductId,
                        ProductName = b.Product.ProductName,
                        BatchNumber = b.BatchNumber,
                        MRP = b.MRP,
                        CostPrice = b.CostPrice,
                        SalePrice = b.SalePrice,
                        Quantity = b.Quantity,
                        RemainingQty = b.RemainingQty,
                        ManufactureDate = b.ManufactureDate,
                        ExpiryDate = b.ExpiryDate
                    })
                    .ToListAsync();

                return ApiResponse(true, "Batches fetched successfully", batches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll Batches");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var batch = await _context.ProductBatches
                    .Include(b => b.Product)
                    .Include(b => b.Firm)
                    .Where(b => b.BatchId == id && b.FirmId == firmId && !b.IsDeleted)
                    .Select(b => new ProductBatchResponseDto
                    {
                        BatchId = b.BatchId,
                        FirmId = b.FirmId,
                        FirmName = b.Firm != null ? b.Firm.FirmName : null,
                        ProductId = b.ProductId,
                        ProductName = b.Product.ProductName,
                        BatchNumber = b.BatchNumber,
                        MRP = b.MRP,
                        CostPrice = b.CostPrice,
                        SalePrice = b.SalePrice,
                        Quantity = b.Quantity,
                        RemainingQty = b.RemainingQty,
                        ManufactureDate = b.ManufactureDate,
                        ExpiryDate = b.ExpiryDate
                    })
                    .FirstOrDefaultAsync();

                if (batch == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                return ApiResponse(true, "Batch fetched successfully", batch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById Batch {BatchId}", id);
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }

       
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductBatchCreateDto dto)
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
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var entity = new ProductBatch
                {
                    FirmId = firmId,
                    ProductId = dto.ProductId,
                    BatchNumber = dto.BatchNumber,
                    MRP = dto.MRP,
                    CostPrice = dto.CostPrice,
                    SalePrice = dto.SalePrice,
                    Quantity = dto.Quantity,
                    RemainingQty = dto.Quantity,
                    ManufactureDate = dto.ManufactureDate,
                    ExpiryDate = dto.ExpiryDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ProductBatches.Add(entity);
                await _context.SaveChangesAsync();

                return ApiResponse(true, "Batch created successfully", new
                {
                    entity.BatchId
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create Batch");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProductBatchUpdateDto dto)
        {
            if (id != dto.BatchId)
                return ApiResponse(false, "BatchId mismatch");

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var entity = await _context.ProductBatches
                    .Where(b => b.BatchId == id && b.FirmId == firmId && !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                entity.BatchNumber = dto.BatchNumber;
                entity.MRP = dto.MRP;
                entity.CostPrice = dto.CostPrice;
                entity.SalePrice = dto.SalePrice;
                entity.ManufactureDate = dto.ManufactureDate;
                entity.ExpiryDate = dto.ExpiryDate;
                entity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Batch updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update Batch {BatchId}", id);
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var entity = await _context.ProductBatches
                    .Where(b => b.BatchId == id && b.FirmId == firmId && !b.IsDeleted)
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Batch deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete Batch {BatchId}", id);
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }
    }
}