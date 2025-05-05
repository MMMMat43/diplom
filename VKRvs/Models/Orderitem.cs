using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VKRvs.Models;

public partial class Orderitem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? MenuitemId { get; set; }

    public int? Quantity { get; set; }

    public int? CustomerId { get; set; } // Привязка к пользователю
    public string? Status { get; set; }

    public virtual Menuitem? Menuitem { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Customer? Customer { get; set; } // Навигационное свойство для пользователя
}
