using grocery_management.Data;
using grocery_management.Models;
using grocery_management.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace grocery_management.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }


        [HttpPost("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordRequest request)

        {
            //var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            //if (currentUserId != userId)
            //    return Forbid();

            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (!result)
                return BadRequest("Current password is incorrect or user not found");

            return NoContent();
        }

    }
}
