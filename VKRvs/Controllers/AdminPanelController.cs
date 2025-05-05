using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VKRvs.Models;

namespace VKRvs.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminPanelController : ControllerBase
    {
        private readonly VkrContext _context;

        public AdminPanelController(VkrContext context)
        {
            _context = context;
        }

        // ========== USERS ==========
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Customers.ToListAsync();
            return Ok(users);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] Customer user)
        {
            _context.Customers.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Customer updated)
        {
            var user = await _context.Customers.FindAsync(id);
            if (user == null) return NotFound();

            user.FirstName = updated.FirstName;
            user.LastName = updated.LastName;
            user.Email = updated.Email;
            user.PhoneNumber = updated.PhoneNumber;
            user.Address = updated.Address;
            user.LoyaltyPoints = updated.LoyaltyPoints;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Customers.FindAsync(id);
            if (user == null) return NotFound();

            _context.Customers.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ========== MENU ==========
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu() => Ok(await _context.Menuitems.ToListAsync());

        [HttpPost("menu")]
        public async Task<IActionResult> CreateMenu([FromBody] Menuitem item)
        {
            _context.Menuitems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("menu/{id}")]
        public async Task<IActionResult> UpdateMenu(int id, [FromBody] Menuitem updated)
        {
            var item = await _context.Menuitems.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = updated.Name;
            item.Description = updated.Description;
            item.Category = updated.Category;
            item.Price = updated.Price;
            item.ImageUrl = updated.ImageUrl;
            item.IsAvailable = updated.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("menu/{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var item = await _context.Menuitems.FindAsync(id);
            if (item == null) return NotFound();

            _context.Menuitems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
