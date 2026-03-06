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
    public class StockAdjustmentsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockAdjustmentsController> _logger;

        public StockAdjustmentsController(
            ApplicationDbContext context,
            ILogger<StockAdjustmentsController> logger)
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

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    try
        //    {
        //        var firmId = GetFirmIdFromToken();
        //        if (firmId == null)
        //            return ApiResponse(false, "Unauthorized", statusCode: 401);

        //        var data = await _context.StockAdjustments
        //            .AsNoTracking()
        //            .Include(x => x.Product)
        //            .Include(x => x.Batch)
        //            .Where(x => x.FirmId == firmId && x.DeletedAt == null)
        //            .OrderByDescending(x => x.AdjustmentId)
        //            .Select(x => new StockAdjustmentResponseDto
        //            {
        //                AdjustmentId = x.AdjustmentId,
        //                FirmId = x.FirmId,
        //                ProductId = x.ProductId,
        //                ProductName = x.Product.ProductName,
        //                BatchId = x.BatchId,
        //                BatchNumber = x.Batch != null ? x.Batch.BatchNumber : null,
        //                AdjustmentType = x.AdjustmentType,
        //                Quantity = x.Quantity,
        //                Reason = x.Reason,
        //                CreatedAt = x.CreatedAt
        //            })
        //            .ToListAsync();

        //        return ApiResponse(true, "Adjustments fetched successfully", data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in GetAll Adjustments");
        //        return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockAdjustmentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(false, "Validation failed",
                    errors: ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList(),
                    statusCode: 400);
            }

            if (dto.Quantity <= 0)
                return ApiResponse(false, "Quantity must be greater than zero");

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var productStock = await _context.ProductStocks
                    .FirstOrDefaultAsync(x =>
                        x.FirmId == firmId &&
                        x.ProductId == dto.ProductId &&
                        !x.IsDeleted);

                //if (productStock == null)
                //    return ApiResponse(false, "Product stock not found");

                bool isIncrease = dto.AdjustmentType?.ToUpper() == "INCREASE";
                bool isDecrease = dto.AdjustmentType?.ToUpper() == "DECREASE";


                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? userId = int.TryParse(userIdStr, out var uid) ? uid : null;

                var adjustment = new StockAdjustment
                {
                    FirmId = firmId,
                    ProductId = dto.ProductId,
                    AdjustmentType = dto.AdjustmentType,
                    Quantity = dto.Quantity,
                    Reason = dto.Reason,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockAdjustments.Add(adjustment);


                _context.StockTransactions.Add(new StockTransaction
                {
                    FirmId = firmId,
                    ProductId = dto.ProductId,
                    TransactionType = "ADJUSTMENT",
                    Quantity = dto.Quantity,
                    IsIncrease = isIncrease,
                    IsDecrease = isDecrease,
                    ReferenceType = "ADJUSTMENT",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });


                if (productStock != null)
                {
                    productStock.Quantity = (productStock.Quantity + (isIncrease ? dto.Quantity : -dto.Quantity));
                    productStock.Remark = "ADJUSTMENT";
                }
                else
                {
                    _context.ProductStocks.Add(new ProductStock
                    {
                        FirmId = firmId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity,
                        Remark = "ADJUSTMENT",
                        CreatedAt = DateTime.UtcNow
                    });
                }


                await _context.SaveChangesAsync();

                return ApiResponse(true, "Stock adjusted successfully", new
                {
                    adjustment.AdjustmentId
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create Adjustment");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }


        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        var firmId = GetFirmIdFromToken();
        //        if (firmId == null)
        //            return ApiResponse(false, "Unauthorized", statusCode: 401);

        //        var item = await _context.StockAdjustments
        //            .FirstOrDefaultAsync(x =>
        //                x.AdjustmentId == id &&
        //                x.FirmId == firmId &&
        //                x.DeletedAt == null);

        //        if (item == null)
        //            return ApiResponse(false, "Record not found", statusCode: 404);

        //        item.DeletedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();

        //        return ApiResponse(true, "Adjustment deleted successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in Delete Adjustment {AdjustmentId}", id);
        //        return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
        //    }
        //}


    }
}