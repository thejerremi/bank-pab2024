namespace Bank.net.Models
{
    public class _RegisterRequest
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public string? Pesel { get; set; }
        public DateTime Dob { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
