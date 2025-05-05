using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class MyOrdersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<Order> Orders { get; set; } = new();
        public List<int> ReviewedItemIds { get; set; } = new();

        public MyOrdersModel(IHttpClientFactory httpClientFactory)
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

            // Загружаем заказы
            var response = await client.GetAsync("http://localhost:5243/api/cart/my-orders");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Orders = JsonSerializer.Deserialize<List<Order>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Order>();
            }

            // Загружаем уже оставленные отзывы
            var reviewsResponse = await client.GetAsync("http://localhost:5243/api/reviews/my");
            if (reviewsResponse.IsSuccessStatusCode)
            {
                var reviewsContent = await reviewsResponse.Content.ReadAsStringAsync();
                var reviews = JsonSerializer.Deserialize<List<Review>>(reviewsContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Review>();
                ReviewedItemIds = reviews.Select(r => r.MenuitemId ?? 0).ToList();
            }
        }

        public IActionResult OnPostWriteReview(int menuItemId)
        {
            return RedirectToPage("/WriteReview", new { menuItemId });
        }
    }
}
