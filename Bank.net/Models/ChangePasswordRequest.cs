namespace Bank.net.Models
{
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmationPassword { get; set; }
    }

}
