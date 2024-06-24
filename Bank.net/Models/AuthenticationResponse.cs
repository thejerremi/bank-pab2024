namespace Bank.net.Models
{
    public class AuthenticationResponse
    {
        public string? Token { get; set; }
        public UserDTO? User { get; set; }
    }
}
