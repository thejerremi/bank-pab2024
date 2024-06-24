namespace Bank.net.Models
{
    public class LoanRequest
    {
        public decimal? Amount { get; set; }
        public int? Length { get; set; } // Term in months
        public string? Type { get; set; }
    }

}
