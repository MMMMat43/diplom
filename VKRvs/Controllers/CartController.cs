/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VKRvs.Models;

namespace VKRvs.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly VkrContext _dbContext;

        public CartController(VkrContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            // Получение идентификатора пользователя из JWT
            int? customerId = GetCustomerIdFromJwt();
            if (customerId == null)
            {
                return Unauthorized("Не удалось определить пользователя.");
            }

            // Поиск элемента корзины
            var cartItem = await _dbContext.Orderitems
                .Include(oi => oi.Menuitem)
                    .ThenInclude(mi => mi.MenuitemIngredients)
                    .ThenInclude(mii => mii.Ingredient)
                .FirstOrDefaultAsync(oi => oi.Id == id && oi.OrderId == null && oi.CustomerId == customerId);

            if (cartItem == null)
            {
                return NotFound("Элемент не найден в корзине.");
            }

            // Возврат ингредиентов на склад
            foreach (var menuItemIngredient in cartItem.Menuitem.MenuitemIngredients)
            {
                menuItemIngredient.Ingredient!.QuantityInStock += menuItemIngredient.QuantityNeeded;
            }

            if (cartItem.Quantity > 1)
            {
                // Уменьшаем количество
                cartItem.Quantity--;
                _dbContext.Orderitems.Update(cartItem);
            }
            else
            {
                // Удаляем элемент, если количество равно 1
                _dbContext.Orderitems.Remove(cartItem);
            }

            await _dbContext.SaveChangesAsync();
            return Ok("Товар обновлён в корзине.");
        }



        // Очистить корзину
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            // Получение идентификатора пользователя из JWT
            int? customerId = GetCustomerIdFromJwt();
            if (customerId == null)
            {
                return Unauthorized("Не удалось определить пользователя.");
            }

            // Поиск всех элементов корзины текущего пользователя
            var cartItems = await _dbContext.Orderitems
                .Include(oi => oi.Menuitem)
                    .ThenInclude(mi => mi.MenuitemIngredients)
                    .ThenInclude(mii => mii.Ingredient)
                .Where(oi => oi.OrderId == null && oi.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return Ok("Корзина уже пуста.");
            }

            // Возврат ингредиентов на склад
            foreach (var cartItem in cartItems)
            {
                foreach (var menuItemIngredient in cartItem.Menuitem.MenuitemIngredients)
                {
                    menuItemIngredient.Ingredient!.QuantityInStock += menuItemIngredient.QuantityNeeded * cartItem.Quantity;
                }
            }

            // Удаление всех элементов корзины
            _dbContext.Orderitems.RemoveRange(cartItems);
            await _dbContext.SaveChangesAsync();

            return Ok("Корзина очищена.");
        }


        // Получить общую сумму корзины
        [HttpGet("total")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _dbContext.Orderitems
                .Where(oi => oi.OrderId == null)
                .SumAsync(oi => oi.Menuitem.Price * oi.Quantity);

            return Ok(new { Total = total });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] Orderitem cartItem)
        {
            // Получение идентификатора пользователя из JWT
            int? customerId = GetCustomerIdFromJwt();
            if (customerId == null)
            {
                return Unauthorized("Не удалось определить пользователя.");
            }

            // Проверка входных данных
            if (cartItem.MenuitemId == null || cartItem.Quantity == null || cartItem.Quantity <= 0)
            {
                return BadRequest("Некорректные данные. Убедитесь, что выбрано блюдо и указано корректное количество.");
            }

            // Получаем данные о блюде с ингредиентами
            var menuItem = await _dbContext.Menuitems
                .Include(mi => mi.MenuitemIngredients)
                .ThenInclude(mii => mii.Ingredient)
                .FirstOrDefaultAsync(mi => mi.Id == cartItem.MenuitemId);

            if (menuItem == null)
            {
                return NotFound("Блюдо не найдено.");
            }

            if (!menuItem.IsAvailable.HasValue || !menuItem.IsAvailable.Value)
            {
                return BadRequest("Блюдо недоступно для заказа.");
            }

            // Проверяем доступность ингредиентов
            foreach (var menuItemIngredient in menuItem.MenuitemIngredients)
            {
                if (menuItemIngredient.QuantityNeeded * cartItem.Quantity > menuItemIngredient.Ingredient!.QuantityInStock)
                {
                    return BadRequest($"Недостаточно ингредиента: {menuItemIngredient.Ingredient.Name}");
                }
            }

            // Вычитаем ингредиенты из склада
            foreach (var menuItemIngredient in menuItem.MenuitemIngredients)
            {
                menuItemIngredient.Ingredient!.QuantityInStock -= menuItemIngredient.QuantityNeeded * cartItem.Quantity;
            }

            // Проверяем, есть ли в корзине это блюдо для текущего пользователя
            var existingCartItem = await _dbContext.Orderitems
                .FirstOrDefaultAsync(oi => oi.MenuitemId == cartItem.MenuitemId && oi.OrderId == null && oi.CustomerId == customerId);

            if (existingCartItem != null)
            {
                // Если блюдо уже есть в корзине, увеличиваем количество
                existingCartItem.Quantity += cartItem.Quantity;
                _dbContext.Orderitems.Update(existingCartItem);
            }
            else
            {
                // Если блюда нет в корзине, добавляем новое
                cartItem.OrderId = null; // Указываем, что это временная корзина
                cartItem.CustomerId = customerId; // Привязываем к пользователю
                await _dbContext.Orderitems.AddAsync(cartItem);
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok("Товар успешно добавлен в корзину.");
            }
            catch (Exception ex)
            {
                // Обработка возможных ошибок сохранения
                return StatusCode(500, $"Ошибка при добавлении в корзину: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
                return Unauthorized("Пользователь не авторизован.");

            var customerIdClaim = identity.FindFirst("CustomerId");
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
                return Unauthorized("ID пользователя не найден.");

            // Получаем товары из корзины пользователя
            var cartItems = await _dbContext.Orderitems
                .Where(oi => oi.CustomerId == customerId && oi.OrderId == null)
                .Include(oi => oi.Menuitem)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest("Корзина пуста.");

            // Расчёт общей стоимости
            var total = cartItems.Sum(ci => ci.Menuitem.Price * ci.Quantity);

            // Создаём новый заказ
            var newOrder = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                LoyaltyDiscount = 0 // или логика расчёта скидки
            };

            await _dbContext.Orders.AddAsync(newOrder);
            await _dbContext.SaveChangesAsync();

            // Привязываем товары корзины к заказу
            foreach (var item in cartItems)
            {
                item.OrderId = newOrder.Id;
                item.Status = "В обработке"; // если уже добавлено поле
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Заказ успешно оформлен.", orderId = newOrder.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            int parsedCustomerId = int.Parse(customerId);

            var cartItems = await _dbContext.Orderitems
                .Include(oi => oi.Menuitem)
                .Where(oi => oi.CustomerId == parsedCustomerId && oi.OrderId == null)
                .ToListAsync();

            return Ok(cartItems);
        }

        private int? GetCustomerIdFromJwt()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
                return null;

            var customerIdClaim = identity.FindFirst("CustomerId");
            if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int customerId))
            {
                return customerId;
            }

            return null;
        }


        [HttpGet("my-orders")]
        public async Task<IActionResult> GetUserOrders()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
                return Unauthorized("Пользователь не авторизован.");

            var customerIdClaim = identity.FindFirst("CustomerId");
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
                return Unauthorized("ID пользователя не найден.");

            var orders = await _dbContext.Orders
                .Include(o => o.Orderitems)
                    .ThenInclude(oi => oi.Menuitem)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }



        [HttpGet("{orderId}/items")]
        [Route("api/orders/{orderId}/items")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var order = await _dbContext.Orders
                .Include(o => o.Orderitems)
                    .ThenInclude(oi => oi.Menuitem)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null) return NotFound("Заказ не найден.");

            var items = order.Orderitems.Select(oi => new
            {
                oi.Id,
                oi.MenuitemId,
                MenuitemName = oi.Menuitem.Name,
                oi.Quantity,
                oi.Status
            });

            return Ok(items);
        }



    }
}
*/


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VKRvs.Models;

namespace VKRvs.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly VkrContext _dbContext;

        public CartController(VkrContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int? GetCustomerIdFromJwt()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
                return null;

            var customerIdClaim = identity.FindFirst("CustomerId");
            if (customerIdClaim != null && int.TryParse(customerIdClaim.Value, out int customerId))
            {
                return customerId;
            }

            return null;
        }

        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var cartItem = await _dbContext.Orderitems
                .Include(oi => oi.Menuitem)
                    .ThenInclude(mi => mi.MenuitemIngredients)
                    .ThenInclude(mii => mii.Ingredient)
                .FirstOrDefaultAsync(oi => oi.Id == id && oi.OrderId == null && oi.CustomerId == customerId);

            if (cartItem == null) return NotFound("Элемент не найден в корзине.");

            foreach (var menuItemIngredient in cartItem.Menuitem.MenuitemIngredients)
            {
                menuItemIngredient.Ingredient!.QuantityInStock += menuItemIngredient.QuantityNeeded;
            }

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                _dbContext.Orderitems.Update(cartItem);
            }
            else
            {
                _dbContext.Orderitems.Remove(cartItem);
            }

            await _dbContext.SaveChangesAsync();
            return Ok("Товар обновлён в корзине.");
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var cartItems = await _dbContext.Orderitems
                .Include(oi => oi.Menuitem)
                    .ThenInclude(mi => mi.MenuitemIngredients)
                    .ThenInclude(mii => mii.Ingredient)
                .Where(oi => oi.OrderId == null && oi.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any()) return Ok("Корзина уже пуста.");

            foreach (var cartItem in cartItems)
            {
                foreach (var menuItemIngredient in cartItem.Menuitem.MenuitemIngredients)
                {
                    menuItemIngredient.Ingredient!.QuantityInStock += menuItemIngredient.QuantityNeeded * cartItem.Quantity;
                }
            }

            _dbContext.Orderitems.RemoveRange(cartItems);
            await _dbContext.SaveChangesAsync();

            return Ok("Корзина очищена.");
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _dbContext.Orderitems
                .Where(oi => oi.OrderId == null)
                .SumAsync(oi => oi.Menuitem.Price * oi.Quantity);

            return Ok(new { Total = total });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] Orderitem cartItem)
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            if (cartItem.MenuitemId == null || cartItem.Quantity == null || cartItem.Quantity <= 0)
                return BadRequest("Некорректные данные.");

            var menuItem = await _dbContext.Menuitems
                .Include(mi => mi.MenuitemIngredients)
                .ThenInclude(mii => mii.Ingredient)
                .FirstOrDefaultAsync(mi => mi.Id == cartItem.MenuitemId);

            if (menuItem == null) return NotFound("Блюдо не найдено.");
            if (!(menuItem.IsAvailable ?? false)) return BadRequest("Блюдо недоступно.");

            foreach (var ing in menuItem.MenuitemIngredients)
            {
                if (ing.QuantityNeeded * cartItem.Quantity > ing.Ingredient!.QuantityInStock)
                    return BadRequest($"Недостаточно ингредиента: {ing.Ingredient.Name}");
            }

            foreach (var ing in menuItem.MenuitemIngredients)
            {
                ing.Ingredient!.QuantityInStock -= ing.QuantityNeeded * cartItem.Quantity;
            }

            var existing = await _dbContext.Orderitems
                .FirstOrDefaultAsync(oi => oi.MenuitemId == cartItem.MenuitemId && oi.OrderId == null && oi.CustomerId == customerId);

            if (existing != null)
            {
                existing.Quantity += cartItem.Quantity;
                _dbContext.Orderitems.Update(existing);
            }
            else
            {
                cartItem.OrderId = null;
                cartItem.CustomerId = customerId;
                await _dbContext.Orderitems.AddAsync(cartItem);
            }

            await _dbContext.SaveChangesAsync();
            return Ok("Добавлено в корзину.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var cartItems = await _dbContext.Orderitems
                .Where(oi => oi.CustomerId == customerId && oi.OrderId == null)
                .Include(oi => oi.Menuitem)
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest("Корзина пуста.");

            var total = cartItems.Sum(ci => ci.Menuitem.Price * ci.Quantity);

            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                LoyaltyDiscount = 0
            };

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                item.OrderId = order.Id;
                item.Status = "В обработке";
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Заказ оформлен.", orderId = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var cartItems = await _dbContext.Orderitems
                .Include(oi => oi.Menuitem)
                .Where(oi => oi.CustomerId == customerId && oi.OrderId == null)
                .ToListAsync();

            return Ok(cartItems);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetUserOrders()
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var orders = await _dbContext.Orders
                .Include(o => o.Orderitems)
                    .ThenInclude(oi => oi.Menuitem)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{orderId}/items")]
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null) return Unauthorized();

            var order = await _dbContext.Orders
                .Include(o => o.Orderitems)
                    .ThenInclude(oi => oi.Menuitem)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null) return NotFound("Заказ не найден.");

            var items = order.Orderitems.Select(oi => new
            {
                oi.Id,
                oi.MenuitemId,
                MenuitemName = oi.Menuitem.Name,
                oi.Quantity,
                oi.Status
            });

            return Ok(items);
        }

        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromForm] Review reviewData)
        {
            var customerId = GetCustomerIdFromJwt();
            if (customerId == null)
                return Unauthorized("Пользователь не авторизован.");

            if (reviewData.MenuitemId == null || reviewData.Rating == null)
                return BadRequest("Некорректные данные отзыва.");

            var review = new Review
            {
                CustomerId = customerId,
                MenuitemId = reviewData.MenuitemId,
                Rating = reviewData.Rating,
                ReviewText = reviewData.ReviewText,
                ReviewDate = DateTime.Now
            };

            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Отзыв успешно добавлен." });
        }

    }
}
