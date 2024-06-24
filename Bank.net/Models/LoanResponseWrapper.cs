namespace Bank.net.Models
{
    public class LoanResponseWrapper
    {
        public int LoanId { get; set; }
        public LoanResponse Loan { get; set; }
        public UserDTO User { get; set; }
    }

}
