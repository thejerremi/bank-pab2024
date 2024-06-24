using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bank.net.Models;
using Bank.net.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;

namespace Bank.net.Services
{
    public interface ILoanService
    {
        Task<LoanResponse> GetUserLoanDetailsAsync(int userId);
        Task<Response> ApplyForLoanAsync(User user, LoanRequest request);
        Task<Response> PayRateAsync(User user, int userId);
        Task<Response> RepaymentAsync(User user, int userId, decimal amount);
    }

    public class LoanService : ILoanService
    {
        private readonly BankDBContext _context;
        private readonly IUserService _userService;

        public LoanService(BankDBContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        private LoanResponse ConvertToResponse(Loan loan)
        {
            return new LoanResponse
            {
                Status = loan.Status,
                Type = loan.Type,
                Length = loan.Length,
                Amount = loan.Amount,
                PaymentLeft = loan.PaymentLeft,
                MonthlyRate = loan.MonthlyRate
            };
        }

        public async Task<LoanResponse> GetUserLoanDetailsAsync(int userId)
        {
            var loan = await _context.Loans.Where(l => l.UserId == userId)
                                           .OrderByDescending(l => l.Id)
                                           .FirstOrDefaultAsync();
            return ConvertToResponse(loan);
        }

        public async Task<Response> ApplyForLoanAsync(User user, LoanRequest request)
        {
            if ((bool)user.HasLoan)
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "User already has a loan." };
            }

            var principal = request.Amount;
            var annualRate = 0.05m;
            var termInMonths = request.Length;
            var monthlyRate = CalculateMonthlyRate((decimal)principal, annualRate, (int)termInMonths);

            var loan = new Loan
            {
                Amount = request.Amount,
                Status = LoanStatus.Pending.ToString(),
                Type = request.Type,
                Length = request.Length,
                MonthlyRate = monthlyRate,
                PaymentLeft = monthlyRate * termInMonths,
                UserId = user.Id
            };
            user.HasLoan = true;
            _context.Users.Update(user);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return new Response { StatusCode = StatusCodes.Status200OK, Message = "Loan application successful." };
        }

        public async Task<Response> PayRateAsync(User user, int userId)
        {
            var userLoan = await _context.Loans.Where(l => l.UserId == userId)
                                               .OrderByDescending(l => l.Id)
                                               .FirstOrDefaultAsync();
            if (userLoan.Status != LoanStatus.Accepted.ToString())
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "This loan cannot be paid off." };
            }
            if (user.Balance < userLoan.MonthlyRate)
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "Insufficient balance to pay monthly rate." };
            }
            if (userLoan.MonthlyRate > userLoan.PaymentLeft)
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "Monthly rate is bigger than required payment left." };
            }
            await _userService.SubtractBalanceAsync(user, (decimal)userLoan.MonthlyRate);
            var transaction = new Transaction
            {
                Type = TransactionType.MonthlyRate.ToString(),
                Amount = userLoan.MonthlyRate,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Description = $"Zapłacono miesięczną ratę pożyczki w wysokości {userLoan.MonthlyRate} PLN."
            };
            _context.Transactions.Add(transaction);
            userLoan.PaymentLeft -= userLoan.MonthlyRate;
            if (userLoan.PaymentLeft < 0.01m)
            {
                user.HasLoan = false;
                userLoan.Status = LoanStatus.PaidOff.ToString();
                _context.Users.Update(user);
            }
            _context.Loans.Update(userLoan);
            await _context.SaveChangesAsync();
            return new Response { StatusCode = StatusCodes.Status200OK, Message = "Monthly rate payment successful." };
        }

        public async Task<Response> RepaymentAsync(User user, int userId, decimal amount)
        {
            var userLoan = await _context.Loans.Where(l => l.UserId == userId)
                                               .OrderByDescending(l => l.Id)
                                               .FirstOrDefaultAsync();
            if (userLoan.Status != LoanStatus.Accepted.ToString())
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "This loan cannot be paid off." };
            }
            if (user.Balance < amount)
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "Insufficient balance to pay this amount of loan repayment." };
            }
            if (amount > userLoan.PaymentLeft)
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "This loan repayment is bigger than required payment left" };
            }
            await _userService.SubtractBalanceAsync(user, amount);
            var transaction = new Transaction
            {
                Type = TransactionType.LoanPayment.ToString(),
                Amount = amount,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Description = $"Spłacono {amount} PLN w ramach pożyczki."
            };
            _context.Transactions.Add(transaction);
            userLoan.PaymentLeft -= amount;
            if (userLoan.PaymentLeft < 0.01m)
            {
                user.HasLoan = false;
                userLoan.Status = LoanStatus.PaidOff.ToString();
                _context.Users.Update(user);
            }
            _context.Loans.Update(userLoan);
            await _context.SaveChangesAsync();
            return new Response { StatusCode = StatusCodes.Status200OK, Message = "Loan repayment successful." };
        }

        private decimal CalculateMonthlyRate(decimal principal, decimal annualRate, int termInMonths)
        {
            var monthlyRate = annualRate / 12;
            var onePlusRateToPower = Math.Pow((double)(1 + monthlyRate), termInMonths);
            var numerator = principal * monthlyRate * (decimal)onePlusRateToPower;
            var denominator = (decimal)onePlusRateToPower - 1;
            return Math.Round(numerator / denominator, 2, MidpointRounding.AwayFromZero);
        }
        
    }
    public enum LoanStatus
    {
        Pending,
        Accepted,
        Rejected,
        PaidOff
    }
}
