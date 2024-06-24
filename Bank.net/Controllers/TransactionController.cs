using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Bank.net.Services;
using Bank.net.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Bank.net.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;
        private readonly _IAuthenticationService _authService;

        public TransactionController(ITransactionService service, _IAuthenticationService authService)
        {
            _service = service;
            _authService = authService;
        }

        [Authorize]
        [HttpGet("last5")]
        public async Task<IActionResult> GetLast5Transactions()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return BadRequest("User not found.");
            }
            var transactions = await _service.GetLast5Transactions(userId);
            return Ok(transactions);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTransactionsWithPagination([FromQuery] int page = 0, [FromQuery] int size = 10)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = _authService.GetUserIdFromEmail(userEmail);
            if (userId == null)
            {
                return BadRequest("User not found.");
            }
            var transactions = await _service.GetTransactionsWithPagination(userId, page, size);
            return Ok(transactions);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] TransactionRequest request, [FromHeader] string authorization)
        {
            var user = _authService.GetUserFromToken(authorization);
            if (user == null)
            {
                return BadRequest("User wasn't found or access token is invalid.");
            }
            await _service.Deposit(request, user);
            return Ok();
        }

        [HttpPost("atm_deposit")]
        public async Task<IActionResult> AtmDeposit([FromBody] TransactionRequest request, [FromHeader] string authorization)
        {
            var user = _authService.GetUserFromToken(authorization);
            if (user == null)
            {
                return BadRequest("User wasn't found or access token is invalid.");
            }
            await _service.AtmDeposit(request, user);
            return Ok();
        }

        [Authorize]
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromHeader] string authorization, [FromBody] TransferRequest request)
        {
            var user = _authService.GetUserFromToken(authorization);
            if (user == null)
            {
                return BadRequest("User wasn't found or access token is invalid.");
            }
            var response = await _service.Transfer(user, request);
            return StatusCode(response.StatusCode, response.Message);
        }
    }
}
