using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace grocery_management.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult ApiResponse(
            bool success,
            string message,
            object data = null,
            string error = null,
            List<string> errors = null,
            int statusCode = 200)
        {
            var response = new
            {
                success,
                message,
                data,
                error,
                errors
            };

            return StatusCode(statusCode, response);
        }
    }
}
