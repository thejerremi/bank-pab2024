using System;
using System.Collections.Generic;

namespace Bank.net.Models;

public partial class User
{
    public int Id { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? Dob { get; set; }

    public string? Email { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? Password { get; set; }

    public string? Pesel { get; set; }

    public string? Role { get; set; }

    public string? AccountNumber { get; set; }

    public bool? HasLoan { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
