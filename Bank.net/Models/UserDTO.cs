using System;

namespace Bank.net.Models
{
    public class UserDTO
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Pesel { get; set; }
        public DateTime? Dob { get; set; }
        public string? Email { get; set; }
        public decimal? Balance { get; set; }
        public string? AccountNumber { get; set; }
        public Boolean? HasLoan { get; set; }
        public string? Role { get; set; }
    }
}
