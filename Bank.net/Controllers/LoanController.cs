using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Bank.net.Services;
using Bank.net.Models;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;

namespace Bank.net.Controllers
{
    [ApiController]
    [Route("api/loans")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _service;
        private readonly _IAuthenticationService _authService;

        public LoanController(ILoanService service, _IAuthenticationService authService)
        {
            _service = service;
            _authService = authService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserLoan()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return BadRequest("User not found.");
            }
            var response = await _service.GetUserLoanDetailsAsync(userId);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("application")]
        public async Task<IActionResult> ApplyForLoan([FromHeader] string authorization, [FromBody] LoanRequest request)
        {
            var user = _authService.GetUserFromToken(authorization);
            if (user == null)
            {
                return BadRequest("User wasn't found or access token is invalid.");
            }
            var response = await _service.ApplyForLoanAsync(user, request);
            return StatusCode(response.StatusCode, response.Message);
        }

        [Authorize]
        [HttpPost("payRate")]
        public async Task<IActionResult> PayRate([FromHeader] string authorization)
        {
            var user = _authService.GetUserFromToken(authorization);
            if (user == null)
            {
                return BadRequest("User wasn't found or access token is invalid.");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return BadRequest("User not found.");
            }
            var response = await _service.PayRateAsync(user, userId);
            return StatusCode(response.StatusCode, response.Message);
        }

        [Authorize]
        [HttpPost("repayment")]
        public async Task<IActionResult> Repayment([FromHeader] string authorization, [FromQuery] decimal amount)
        {
            var user = _authService.GetUserFromToken(authorization);
            if (user == null)
            {
                return BadRequest("User wasn't found or access token is invalid.");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return BadRequest("User not found.");
            }
            var response = await _service.RepaymentAsync(user, userId, amount);
            return StatusCode(response.StatusCode, response.Message);
        }
    }
}
