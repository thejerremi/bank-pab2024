using Bank.net.Models;
using Bank.net.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Bank.net.Services
{
    public interface ITransactionService
    {
        Task<List<TransactionResponse>> GetLast5Transactions(int userId);
        Task<PaginatedList<TransactionResponse>> GetTransactionsWithPagination(int userId, int page, int size);
        Task Deposit(TransactionRequest request, User user);
        Task AtmDeposit(TransactionRequest request, User user);
        Task<Response> Transfer(User user, TransferRequest request);
    }

    public class TransactionService : ITransactionService
    {
        private readonly BankDBContext _context;
        private readonly IUserService _userService;

        public TransactionService(BankDBContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<List<TransactionResponse>> GetLast5Transactions(int userId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            return transactions.Select(t => ConvertToResponse(t)).ToList();
        }

        public async Task<PaginatedList<TransactionResponse>> GetTransactionsWithPagination(int userId, int page, int size)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();

            var totalCount = await _context.Transactions.CountAsync(t => t.UserId == userId);

            return new PaginatedList<TransactionResponse>
            {
                Items = transactions.Select(t => ConvertToResponse(t)).ToList(),
                PageIndex = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)size)
            };
        }

        public async Task Deposit(TransactionRequest request, User user)
        {
            var deposit = new Transaction
            {
                Type = TransactionType.Deposit.ToString(),
                Amount = request.Amount,
                User = user,
                CreatedAt = DateTime.UtcNow,
                Description = $"Wpłacono {request.Amount} złotych."
            };

            await _userService.AddBalanceAsync(user, request.Amount);
            _context.Transactions.Add(deposit);
            await _context.SaveChangesAsync();
        }

        public async Task AtmDeposit(TransactionRequest request, User user)
        {
            var deposit = new Transaction
            {
                Type = TransactionType.AtmDeposit.ToString(),
                Amount = request.Amount,
                User = user,
                CreatedAt = DateTime.UtcNow,
                Description = $"Wpłacono {request.Amount} złotych we wpłatomacie."
            };

            await _userService.AddBalanceAsync(user, request.Amount);
            _context.Transactions.Add(deposit);
            await _context.SaveChangesAsync();
        }

        public async Task<Response> Transfer(User user, TransferRequest request)
        {
            if (user.Balance < request.Amount)
            {
                return new Response { StatusCode = 400, Message = "Insufficient balance." };
            }

            if (request.AccountNumberDest.Length != 26)
            {
                return new Response { StatusCode = 400, Message = "Incorrect account number." };
            }

            var userDestination = await _context.Users.SingleOrDefaultAsync(u => u.AccountNumber == request.AccountNumberDest);

            if (userDestination != null)
            {
                await _userService.AddBalanceAsync(userDestination, request.Amount);
            }

            await _userService.SubtractBalanceAsync(user, request.Amount);

            var recipientDescription = userDestination != null
                ? $"{userDestination.Firstname} {userDestination.Lastname}, numer konta: {userDestination.AccountNumber}"
                : $"numer konta: {request.AccountNumberDest}";

            var transferSent = new Transaction
            {
                Type = TransactionType.Transfer.ToString(),
                Amount = request.Amount,
                User = user,
                CreatedAt = DateTime.UtcNow,
                Recipient = recipientDescription,
                Description = $"Przelano {request.Amount} złotych na konto: {request.AccountNumberDest}"
            };

            if (userDestination != null)
            {
                var transferReceived = new Transaction
                {
                    Type = TransactionType.UserTransfer.ToString(),
                    Amount = request.Amount,
                    User = userDestination,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"Otrzymano {request.Amount} złotych od {user.Firstname} {user.Lastname}, numer konta: {user.AccountNumber}"
                };
                _context.Transactions.Add(transferReceived);
            }

            _context.Transactions.Add(transferSent);
            await _context.SaveChangesAsync();

            return new Response { StatusCode = 200, Message = "Transfer successful." };
        }

        private TransactionResponse ConvertToResponse(Transaction transaction)
        {
            return new TransactionResponse
            {
                Type = transaction.Type,
                Amount = transaction.Amount,
                CreatedAt = transaction.CreatedAt,
                Recipient = transaction.Recipient,
                Description = transaction.Description
            };
        }
    }
    
        public enum TransactionType
        {
            Deposit,         // WPŁATA
            AtmDeposit,      // WPŁATA Z WPŁATOMATU
            UserTransfer,    // WPŁYW OD INNEGO UŻYTKOWNIKA
            Withdraw,        // WYPŁATA
            Transfer,        // PRZELEW
            Loan,            // POŻYCZKA
            Interest,        // ODSETKI
            MonthlyRate,     // MIESIĘCZNA RATA
            LoanPayment      // SPŁATA POŻYCZKI
        }
    

}
