using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bank.net.Models;
using Microsoft.AspNetCore.Http;

namespace Bank.net.Services
{
    public interface IAdminService
    {
        Task<List<LoanResponseWrapper>> FindPendingLoansAsync();
        Task<Response> ChangeLoanStatusAsync(string decision, int loanId);
    }

    public class AdminService : IAdminService
    {
        private readonly BankDBContext _context;

        public AdminService(BankDBContext context)
        {
            _context = context;
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

        private LoanResponseWrapper ConvertToWrapperResponse(Loan loan)
        {
            var loanResponse = ConvertToResponse(loan);
            var user =  _context.Users.Find(loan.UserId);
            var userDTO = ConvertToUserDTO(user);
            return new LoanResponseWrapper
            {
                LoanId = loan.Id,
                Loan = loanResponse,
                User = userDTO
            };
        }

        private UserDTO ConvertToUserDTO(User user)
        {
            return new UserDTO
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                Pesel = user.Pesel,
                Dob = user.Dob,
                Balance = user.Balance,
                AccountNumber = user.AccountNumber,
                HasLoan = user.HasLoan,
                Role = user.Role
            };
        }

        public async Task<List<LoanResponseWrapper>> FindPendingLoansAsync()
        {
            var loans = await _context.Loans.Where(l => l.Status == LoanStatus.Pending.ToString()).ToListAsync();
            return loans.Select(ConvertToWrapperResponse).ToList();
        }

        public async Task<Response> ChangeLoanStatusAsync(string decision, int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null)
            {
                return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "Loan with that id was not found." };
            }

            if (decision == "Accept")
            {
                loan.Status = LoanStatus.Accepted.ToString();
                var user = _context.Users.Find(loan.UserId);
                user.HasLoan = true;
                user.Balance += loan.Amount;

                var transaction = new Transaction
                {
                    Type = TransactionType.Loan.ToString(),
                    Amount = loan.Amount,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"Otrzymano {loan.Amount} z tytułu akceptacji pożyczki."
                };

                _context.Users.Update(user);
                _context.Loans.Update(loan);
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return new Response { StatusCode = StatusCodes.Status200OK, Message = "Loan accepted successfully." };
            }
            else if (decision == "Reject")
            {
                loan.Status = LoanStatus.Rejected.ToString();
                var user = _context.Users.Find(loan.UserId);

                user.HasLoan = false;

                _context.Users.Update(user);
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();

                return new Response { StatusCode = StatusCodes.Status200OK, Message = "Loan rejected successfully." };
            }

            return new Response { StatusCode = StatusCodes.Status400BadRequest, Message = "Wrong decision type or loan not found." };
        }
    }
}
