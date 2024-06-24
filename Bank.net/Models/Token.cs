using System;
using System.Collections.Generic;

namespace Bank.net.Models;

public partial class Token
{
    public int Id { get; set; }

    public bool Expired { get; set; }

    public bool Revoked { get; set; }

    public string? Token1 { get; set; }

    public string? TokenType { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
