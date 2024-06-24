namespace Bank.net.Models
{
    public class LoanResponse
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public int? Length { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PaymentLeft { get; set; }
        public decimal? MonthlyRate { get; set; }
    }

}
