using System;
using System.Collections.Generic;

namespace Bank.net.Models;

public partial class Loan
{
    public int Id { get; set; }

    public decimal? Amount { get; set; }

    public int? Length { get; set; }

    public decimal? MonthlyRate { get; set; }

    public decimal? PaymentLeft { get; set; }

    public string? Status { get; set; }

    public string? Type { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
