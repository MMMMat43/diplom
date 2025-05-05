using System;
using System.Collections.Generic;

namespace VKRvs.Models;

public partial class Ingredient
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Unit { get; set; }

    public decimal? QuantityInStock { get; set; }

    public virtual ICollection<MenuitemIngredient> MenuitemIngredients { get; set; } = new List<MenuitemIngredient>();
}
