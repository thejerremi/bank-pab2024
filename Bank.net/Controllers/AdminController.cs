using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Bank.net.Services;
using Bank.net.Models;
using System.Collections.Generic;

namespace Bank.net.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        [HttpGet("loans")]
        public async Task<IActionResult> FindPendingLoans()
        {
            var response = await _service.FindPendingLoansAsync();
            return Ok(response);
        }

        [HttpPost("loans")]
        public async Task<IActionResult> ChangeLoanStatus([FromQuery] string decision, [FromQuery] int loanId)
        {
            var response = await _service.ChangeLoanStatusAsync(decision, loanId);
            return StatusCode(response.StatusCode, response.Message);
        }
    }
}
