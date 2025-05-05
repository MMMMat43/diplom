using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VKRvs.Models;

public partial class Menuitem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    public bool? IsAvailable { get; set; }

    public string? ImageUrl { get; set; }

    public double? AverageRating { get; set; }

    public virtual ICollection<MenuitemIngredient> MenuitemIngredients { get; set; } = new List<MenuitemIngredient>();

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
