namespace Bank.net.Models
{
    public class TransferRequest
    {
        public decimal Amount { get; set; }
        public string AccountNumberDest { get; set; }
    }

}
