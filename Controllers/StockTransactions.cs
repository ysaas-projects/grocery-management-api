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
    public class StockTransactionsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockTransactionsController> _logger;

        public StockTransactionsController(
            ApplicationDbContext context,
            ILogger<StockTransactionsController> logger)
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

                var data = await _context.StockTransactions
                    .Include(x => x.Product)
                    .Include(x => x.Batch)
                    .Include(x => x.Firm)
                    .Where(x => x.FirmId == firmId)
                    .OrderByDescending(x => x.TransactionId)
                    .Select(x => new StockTransactionResponseDto
                    {
                        TransactionId = x.TransactionId,
                        FirmId = x.FirmId,
                        FirmName = x.Firm != null ? x.Firm.FirmName : null,
                        ProductId = x.ProductId,
                        ProductName = x.Product.ProductName,
                        BatchId = x.BatchId,
                        BatchNumber = x.Batch != null ? x.Batch.BatchNumber : null,
                        TransactionType = x.TransactionType,
                        Quantity = x.Quantity,
                        IsIncrease = x.IsIncrease,
                        ReferenceId = x.ReferenceId,
                        ReferenceType = x.ReferenceType,
                        Notes = x.Notes,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "Transactions fetched successfully", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll Transactions");
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

                var query = _context.StockTransactions
                    .Include(x => x.Product)
                    .Include(x => x.Batch)
                    .Where(x => x.FirmId == firmId);

                if (productId.HasValue)
                    query = query.Where(x => x.ProductId == productId);

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.TransactionId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new StockTransactionResponseDto
                    {
                        TransactionId = x.TransactionId,
                        FirmId = x.FirmId,
                        ProductId = x.ProductId,
                        ProductName = x.Product.ProductName,
                        BatchId = x.BatchId,
                        BatchNumber = x.Batch != null ? x.Batch.BatchNumber : null,
                        TransactionType = x.TransactionType,
                        Quantity = x.Quantity,
                        IsIncrease = x.IsIncrease,
                        ReferenceId = x.ReferenceId,
                        ReferenceType = x.ReferenceType,
                        Notes = x.Notes,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse(true, "Transactions retrieved successfully", new
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
                _logger.LogError(ex, "Error in GetPaginated Transactions");
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

                var item = await _context.StockTransactions
                    .Include(x => x.Product)
                    .Include(x => x.Batch)
                    .Include(x => x.Firm)
                    .Where(x => x.TransactionId == id && x.FirmId == firmId)
                    .Select(x => new StockTransactionResponseDto
                    {
                        TransactionId = x.TransactionId,
                        FirmId = x.FirmId,
                        FirmName = x.Firm != null ? x.Firm.FirmName : null,
                        ProductId = x.ProductId,
                        ProductName = x.Product.ProductName,
                        BatchId = x.BatchId,
                        BatchNumber = x.Batch != null ? x.Batch.BatchNumber : null,
                        TransactionType = x.TransactionType,
                        Quantity = x.Quantity,
                        IsIncrease = x.IsIncrease,
                        ReferenceId = x.ReferenceId,
                        ReferenceType = x.ReferenceType,
                        Notes = x.Notes,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                    return ApiResponse(false, "Record not found", statusCode: 404);

                return ApiResponse(true, "Transaction fetched successfully", item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById Transaction {TransactionId}", id);
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockTransactionCreateDto dto)
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

                var entity = new StockTransaction
                {
                    FirmId = firmId,
                    ProductId = dto.ProductId,
                    BatchId = dto.BatchId,
                    TransactionType = dto.TransactionType,
                    Quantity = dto.Quantity,
                    IsIncrease = dto.IsIncrease,
                    ReferenceId = dto.ReferenceId,
                    ReferenceType = dto.ReferenceType,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockTransactions.Add(entity);
                await _context.SaveChangesAsync();

                return ApiResponse(true, "Transaction created successfully", new
                {
                    entity.TransactionId
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create Transaction");
                return ApiResponse(false, "Something went wrong", error: ex.Message, statusCode: 500);
            }
        }
    }
}