using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VKRvs.Models;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly VkrContext _dbContext;

    public ReviewController(VkrContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> LeaveReview([FromBody] Review review)
    {
        var customerId = GetCustomerIdFromJwt();
        if (customerId == null)
            return Unauthorized();

        var hasOrdered = await _dbContext.Orders
            .Include(o => o.Orderitems)
            .AnyAsync(o => o.CustomerId == customerId &&
                           o.Orderitems.Any(oi => oi.MenuitemId == review.MenuitemId));

        if (!hasOrdered)
            return Forbid("Вы можете оставить отзыв только после покупки блюда.");

        review.CustomerId = customerId;
        review.ReviewDate = DateTime.Now;

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        await UpdateMenuItemRating((int)review.MenuitemId!);

        return Ok("Отзыв успешно добавлен.");
    }

    private async Task UpdateMenuItemRating(int menuItemId)
    {
        var reviews = await _dbContext.Reviews
            .Where(r => r.MenuitemId == menuItemId && r.Rating != null)
            .ToListAsync();

        if (reviews.Any())
        {
            var avgRating = reviews.Average(r => r.Rating!.Value);
            var menuItem = await _dbContext.Menuitems.FindAsync(menuItemId);
            if (menuItem != null)
            {
                menuItem.AverageRating = Math.Round(avgRating, 1);
                await _dbContext.SaveChangesAsync();
            }
        }
    }

    private int? GetCustomerIdFromJwt()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        var claim = identity?.FindFirst("CustomerId");
        return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
    }
}
