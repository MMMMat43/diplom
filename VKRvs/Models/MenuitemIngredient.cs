using System;
using System.Collections.Generic;

namespace VKRvs.Models;

public partial class MenuitemIngredient
{
    public int Id { get; set; }

    public int? MenuitemId { get; set; }

    public int? IngredientId { get; set; }

    public decimal? QuantityNeeded { get; set; }

    public virtual Ingredient? Ingredient { get; set; }

    public virtual Menuitem? Menuitem { get; set; }
}
