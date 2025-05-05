using System;
using System.Collections.Generic;

namespace VKRvs.Models;

public partial class Review
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? MenuitemId { get; set; }

    public int? Rating { get; set; }

    public string? ReviewText { get; set; }

    public DateTime? ReviewDate { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Menuitem? Menuitem { get; set; }
}
