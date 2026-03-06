using grocery_management.Data;
using grocery_management.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class SalesController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesController> _logger;

        public SalesController(
          ApplicationDbContext context,
          ILogger<SalesController> logger)
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


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSaleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiResponse(false, "Validation failed",
                    errors: ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList(),
                    statusCode: 400);
            }

            if (dto.Items == null || !dto.Items.Any())
                return ApiResponse(false, "Cart cannot be empty");

            try
            {
                var firmId = GetFirmIdFromToken();
                if (firmId == null)
                    return ApiResponse(false, "Unauthorized", statusCode: 401);

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? userId = int.TryParse(userIdStr, out var uid) ? uid : null;

                var invoiceNumber = $"INV-{DateTime.UtcNow.Ticks}";

                var sale = new Sale
                {
                    FirmId = firmId.Value,
                    InvoiceNumber = invoiceNumber,
                    Subtotal = dto.Subtotal,
                    GST = dto.GST,
                    Total = dto.Total,
                    PaymentMethod = dto.PaymentMethod,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                foreach (var item in dto.Items)
                {
                    if (item.Quantity <= 0)
                        return ApiResponse(false, "Invalid quantity");

                    var productStock = await _context.ProductStocks
                        .FirstOrDefaultAsync(x =>
                            x.FirmId == firmId &&
                            x.ProductId == item.ProductId &&
                            !x.IsDeleted);

                    if (productStock == null || productStock.Quantity < item.Quantity)
                        return ApiResponse(false, "Insufficient stock");

                    // SaleItem
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Price * item.Quantity
                    };

                    _context.SaleItems.Add(saleItem);

                    // STOCK TRANSACTION
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        FirmId = firmId,
                        ProductId = item.ProductId,
                        TransactionType = "SALE",
                        Quantity = item.Quantity,
                        IsIncrease = false,
                        IsDecrease = true,
                        ReferenceId = sale.SaleId,
                        ReferenceType = "SALE",
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow
                    });

                    // UPDATE PRODUCT STOCK
                    productStock.Quantity -= item.Quantity;
                    productStock.Remark = "SALE";
                }

                await _context.SaveChangesAsync();

                return ApiResponse(true, "Sale completed successfully", new
                {
                    sale.SaleId,
                    sale.InvoiceNumber
                }, statusCode: 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create Sale");
                return ApiResponse(false, "Something went wrong",
                    error: ex.Message, statusCode: 500);
            }
        }

    }
}
