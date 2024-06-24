using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Bank.net.Services;
using Bank.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Bank.net.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly _IAuthenticationService _service;

        public AuthenticationController(_IAuthenticationService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] _RegisterRequest request)
        {
            var response = await _service.RegisterAsync(request);
            if (response == null)
            {
                return Conflict("User already exists.");
            }
            return Ok(response);
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequest request)
        {
            var response = await _service.AuthenticateAsync(request);
            if (response == null)
            {
                return Unauthorized("Invalid credentials.");
            }
            return Ok(response);
        }

        [HttpGet("user-token")]
        public async Task<IActionResult> GetUserByToken([FromHeader] string authorization)
        {
            var response = await _service.FindUserDTOByTokenAsync(authorization);
            if (response == null)
            {
                return BadRequest("Invalid token.");
            }
            return Ok(response);
        }

        [HttpPost("delete-account")]
        public async Task<IActionResult> DeleteUser([FromHeader] string authorization, [FromBody] AuthenticationRequest request)
        {
            var response = await _service.DeleteUserAsync(authorization, request);
            if (!response.IsSuccess)
            {
                return StatusCode(response.StatusCode, response.Message);
            }
            return Ok(response.Message);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromHeader] string authorization)
        {
            await _service.LogoutAsync(authorization);
            return Ok("Logged out successfully.");
        }

    }
}
