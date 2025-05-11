using Microsoft.AspNetCore.Mvc;
using VKRvs.Models;
using Microsoft.EntityFrameworkCore;

namespace VKRvs.Controllers;

[Route("api/ingredients")]
[ApiController]
public class IngredientController : ControllerBase
{
    private readonly VkrContext _context;

    public IngredientController(VkrContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients()
    {
        return await _context.Ingredients.ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> AddIngredient([FromBody] Ingredient ingredient)
    {
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
        return Ok(ingredient);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIngredient(int id)
    {
        var ing = await _context.Ingredients.FindAsync(id);
        if (ing == null) return NotFound();

        _context.Ingredients.Remove(ing);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}/increase")]
    public async Task<IActionResult> IncreaseQuantity(int id, [FromBody] decimal amount)
    {
        var ing = await _context.Ingredients.FindAsync(id);
        if (ing == null) return NotFound();

        ing.QuantityInStock += amount;
        await _context.SaveChangesAsync();
        return Ok(ing);
    }

    [HttpPut("{id}/decrease")]
    public async Task<IActionResult> DecreaseQuantity(int id, [FromBody] decimal amount)
    {
        var ing = await _context.Ingredients.FindAsync(id);
        if (ing == null) return NotFound();

        ing.QuantityInStock = Math.Max(0, (decimal)(ing.QuantityInStock - amount));
        await _context.SaveChangesAsync();
        return Ok(ing);
    }
}
