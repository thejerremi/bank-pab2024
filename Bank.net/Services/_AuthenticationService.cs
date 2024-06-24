using Bank.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;

namespace Bank.net.Services
{
    public interface _IAuthenticationService
    {
        Task<AuthenticationResponse> RegisterAsync(_RegisterRequest request);
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<UserDTO> FindUserDTOByTokenAsync(string token);
        Task<Response> DeleteUserAsync(string token, AuthenticationRequest request);
        Task LogoutAsync(string token);
        int GetUserIdFromEmail(string email);
        User GetUserFromToken(string token);

    }

    public class _AuthenticationService : _IAuthenticationService
    {
        private readonly BankDBContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;

        public _AuthenticationService(BankDBContext context, IPasswordHasher<User> passwordHasher, IConfiguration config)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        public async Task<AuthenticationResponse> RegisterAsync(_RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return null;
            }

            var user = new User
            {
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Pesel = request.Pesel,
                Dob = request.Dob,
                Email = request.Email,
                HasLoan = false,
                Password = _passwordHasher.HashPassword(null, request.Password),
                Balance = 300,
                AccountNumber = GenerateRandomAccountNumber().Substring(0,26),
                Role = "USER"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token = GenerateToken(user);
            _context.Tokens.Add(new Token { Token1 = token, UserId = user.Id });
            await _context.SaveChangesAsync();

            return new AuthenticationResponse { Token = token, User = ConvertToDTO(user) };
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || _passwordHasher.VerifyHashedPassword(null, user.Password, request.Password) != PasswordVerificationResult.Success)
            {
                return null;
            }

            var token = GenerateToken(user);
            _context.Tokens.Add(new Token { Token1 = token, UserId = user.Id });
            await _context.SaveChangesAsync();
            return new AuthenticationResponse { Token = token, User = ConvertToDTO(user) };
        }

        public async Task<UserDTO> FindUserDTOByTokenAsync(string token)
        {
            var user = GetUserFromToken(token);
            return user == null ? null : ConvertToDTO(user);
        }

        public async Task<Response> DeleteUserAsync(string token, AuthenticationRequest request)
        {
            var user = GetUserFromToken(token);
            if (user == null)
            {
                return new Response { IsSuccess = false, StatusCode = StatusCodes.Status401Unauthorized, Message = "Couldn't authorize user." };
            }

            if (_passwordHasher.VerifyHashedPassword(null, user.Password, request.Password) != PasswordVerificationResult.Success)
            {
                return new Response { IsSuccess = false, StatusCode = StatusCodes.Status401Unauthorized, Message = "Invalid password." };
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return new Response { IsSuccess = true, StatusCode = StatusCodes.Status200OK, Message = "Deleted." };
        }

        public async Task LogoutAsync(string token)
        {
            var jwt = token.StartsWith("Bearer ") ? token.Substring(7) : token;
            var storedToken = _context.Tokens.SingleOrDefault(t => t.Token1 == jwt);
            if (storedToken != null)
            {
                storedToken.Expired = true;
                storedToken.Revoked = true;
                _context.Tokens.Update(storedToken);
                await _context.SaveChangesAsync();
            }
        }


        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.Firstname),
                new Claim(ClaimTypes.Surname, user.Lastname),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public User GetUserFromToken(string token)
        {
            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
            {
                return null;
            }

            token = token.Substring(7);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            return _context.Users.SingleOrDefault(u => u.Email == email);
        }
        public int GetUserIdFromEmail(string email)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            return user == null ? -1 : user.Id;
        }
        private UserDTO ConvertToDTO(User user)
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
        private string GenerateRandomAccountNumber()
        {
            Random random = new Random();
            return string.Join("", Enumerable.Range(0, 26).Select(_ => random.Next(0, 26).ToString()));
        }
    }

    public class Response
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }
}
