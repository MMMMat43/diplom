using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class CartModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public bool IsOrderPlaced { get; set; } = false;
        public List<Orderitem> CartItems { get; set; }

        public CartModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                CartItems = new List<Orderitem>(); // Пустая корзина
                return;
            }

            var response = await client.GetAsync("http://localhost:5243/api/cart");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                CartItems = JsonSerializer.Deserialize<List<Orderitem>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                CartItems = new List<Orderitem>(); // Пустая корзина
            }
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();

            // Получаем токен из сессии
            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Пустая корзина, если пользователь не авторизован
                return RedirectToPage("/Cart");
            }

            // Отправляем запрос на удаление элемента из корзины
            var response = await client.DeleteAsync($"http://localhost:5243/api/cart/remove/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Cart");
            }

            // Если не удалось удалить элемент
            return BadRequest("Не удалось удалить элемент.");
        }

        public async Task<IActionResult> OnPostOrderAsync()
        {
            var client = _httpClientFactory.CreateClient();

            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Если пользователь не авторизован, перенаправляем на страницу входа
                return RedirectToPage("/Login");
            }

            // Отправляем запрос на сервер для оформления заказа
            var response = await client.PostAsync("http://localhost:5243/api/cart", null);

            if (response.IsSuccessStatusCode)
            {
                IsOrderPlaced = true;
                CartItems = new List<Orderitem>();
                TempData["OrderMessage"] = "Заказ успешно оформлен!";
                return RedirectToPage("/Cart");
            }

            // Если оформление заказа не удалось
            ModelState.AddModelError(string.Empty, "Не удалось оформить заказ. Пожалуйста, попробуйте снова.");
            return Page();
        }


        public async Task<IActionResult> OnPostClearAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // Получаем токен из сессии
            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Пустая корзина, если пользователь не авторизован
                return RedirectToPage("/Cart");
            }

            // Отправляем запрос на очистку корзины
            var response = await client.DeleteAsync("http://localhost:5243/api/cart/clear");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Cart");
            }



            // Если не удалось очистить корзину
            return BadRequest("Не удалось очистить корзину.");
        }

    }
}
