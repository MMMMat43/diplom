using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VKRvs.Models;

namespace VKRvs.Controllers
{
    [Route("api/kitchen")]
    [ApiController]
    public class KitchenController : ControllerBase
    {
        private readonly VkrContext _context;

        public KitchenController(VkrContext context)
        {
            _context = context;
        }

        // Получить все активные заказы
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Orderitems)
                    .ThenInclude(oi => oi.Menuitem)
                .Where(o => o.Orderitems.Any()) // Только заказы с блюдами
                .ToListAsync();

            return Ok(orders);
        }

        // Обновить статус блюда
        [HttpPut("orderitem/{id}/status")]
        public async Task<IActionResult> UpdateItemStatus(int id, [FromBody] string newStatus)
        {
            var item = await _context.Orderitems.FindAsync(id);
            if (item == null)
                return NotFound("Блюдо не найдено.");

            item.Status = newStatus; // Предполагаем, что поле Status уже добавлено в Orderitem
            await _context.SaveChangesAsync();

            return Ok("Статус блюда обновлён.");
        }
    }
}
