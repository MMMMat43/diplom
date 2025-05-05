using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VKRvs.DTO;
using VKRvs.Mappers;
using VKRvs.Models;

namespace VKRvs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly VkrContext _context;

        public MenuController(VkrContext context)
        {
            _context = context;
        }

        // GET: api/menu
        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            var menuItems = await _context.Menuitems
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Description,
                    m.Price,
                    m.Category,
                    m.ImageUrl,
                    AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : (double?)null
                })
                .ToListAsync();

            return Ok(menuItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            var item = await _context.Menuitems
                .Where(m => m.Id == id)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Description,
                    m.Price,
                    m.Category,
                    m.ImageUrl,
                    AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : (double?)null
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredMenu(string? category, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Menuitems.AsQueryable();

            // Фильтрация по категории
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(mi => mi.Category == category);
            }

            // Фильтрация по минимальной цене
            if (minPrice.HasValue)
            {
                query = query.Where(mi => mi.Price >= minPrice.Value);
            }

            // Фильтрация по максимальной цене
            if (maxPrice.HasValue)
            {
                query = query.Where(mi => mi.Price <= maxPrice.Value);
            }

            var filteredMenu = await query.ToListAsync();
            return Ok(filteredMenu);
        }

    }
}
