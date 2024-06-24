using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Bank.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.net.Services
{
    public interface IUserService
    {
        Task<Response> ChangePasswordAsync(ChangePasswordRequest request, int userId);
        Task<Response> UpdateDetailsAsync(UserDTO request, int userId);
        Task AddBalanceAsync(User user, decimal amount);
        Task SubtractBalanceAsync(User user, decimal amount);
    }

    public class UserService : IUserService
    {
        private readonly BankDBContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(BankDBContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Response> ChangePasswordAsync(ChangePasswordRequest request, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new Response { IsSuccess = false, Message = "User not found" };
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.CurrentPassword);
            if (result != PasswordVerificationResult.Success)
            {
                return new Response { IsSuccess = false, Message = "Wrong password" };
            }

            if (request.NewPassword != request.ConfirmationPassword)
            {
                return new Response { IsSuccess = false, Message = "Passwords do not match" };
            }

            user.Password = _passwordHasher.HashPassword(user, request.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new Response { IsSuccess = true };
        }

        public async Task<Response> UpdateDetailsAsync(UserDTO request, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new Response { IsSuccess = false, Message = "User not found" };
            }

            if (request.Firstname != null)
            {
                user.Firstname = request.Firstname;
            }

            if (request.Lastname != null)
            {
                user.Lastname = request.Lastname;
            }

            if (request.Dob != null)
            {
                user.Dob = request.Dob;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new Response { IsSuccess = true };
        }

        public async Task AddBalanceAsync(User user, decimal amount)
        {
            user.Balance += amount;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task SubtractBalanceAsync(User user, decimal amount)
        {
            user.Balance -= amount;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
