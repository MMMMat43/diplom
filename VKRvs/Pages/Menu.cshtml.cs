using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class MenuModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<Menuitem> MenuItems { get; set; }
        public string? SelectedCategory { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SearchQuery { get; set; }

        public MenuModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            MenuItems = new List<Menuitem>();
        }

        public async Task OnGetAsync(string? category, decimal? minPrice, decimal? maxPrice, string? searchQuery)
        {
            SelectedCategory = category;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            SearchQuery = searchQuery;

            var client = _httpClientFactory.CreateClient();

            // Формируем URL с параметрами фильтрации и поиска
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={category}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");
            if (!string.IsNullOrEmpty(searchQuery)) queryParams.Add($"searchQuery={searchQuery}");

            var url = "http://localhost:5243/api/menu/filter";
            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                MenuItems = JsonSerializer.Deserialize<List<Menuitem>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                MenuItems = new List<Menuitem>();
            }
        }



        public async Task<IActionResult> OnPostAddToCartAsync(int menuItemId, int quantity)
        {
            var client = _httpClientFactory.CreateClient();
            // Получение токена из сессии
            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var cartItem = new { MenuitemId = menuItemId, Quantity = quantity };

            var response = await client.PostAsJsonAsync("http://localhost:5243/api/cart/add", cartItem);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Menu");
            }

            return BadRequest("Не удалось добавить товар в корзину.");
        }

    }
}
