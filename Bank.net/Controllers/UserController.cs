using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Bank.net.Services;
using Bank.net.Models;

namespace Bank.net.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly _IAuthenticationService _authService;


        public UserController(IUserService service, _IAuthenticationService authService)
        {
            _service = service;
            _authService = authService;
        }

        [Authorize]
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            int userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _service.ChangePasswordAsync(request, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok();
        }

        [Authorize]
        [HttpPatch("update-details")]
        public async Task<IActionResult> UpdateDetails([FromBody] UserDTO request)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            int userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _service.UpdateDetailsAsync(request, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok();
        }
    }
}
