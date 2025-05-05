using System;
using System.Collections.Generic;

namespace VKRvs.Models;

public partial class LoyaltyTransaction
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public string? TransactionType { get; set; }

    public int? Points { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int? OrderId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Order? Order { get; set; }
}
