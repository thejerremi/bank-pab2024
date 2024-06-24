namespace Bank.net.Models
{
    public class TransactionResponse
    {
        public string? Type { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Recipient { get; set; }
        public string? Description { get; set; }
    }

}
