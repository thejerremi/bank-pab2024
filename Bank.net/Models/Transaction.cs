﻿using System;
using System.Collections.Generic;

namespace Bank.net.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Description { get; set; }

    public string? Recipient { get; set; }

    public string? Type { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
