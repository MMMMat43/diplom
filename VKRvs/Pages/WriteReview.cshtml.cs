using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class WriteReviewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WriteReviewModel> _logger;

        public WriteReviewModel(IHttpClientFactory httpClientFactory, ILogger<WriteReviewModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int MenuItemId { get; set; }

        [BindProperty]
        public int? Rating { get; set; }

        [BindProperty]
        public string? ReviewText { get; set; }

        public string? MenuItemName { get; set; }

        public string? Message { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Загружаем название блюда
            var response = await client.GetAsync($"http://localhost:5243/api/menu/{MenuItemId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var menuItem = JsonSerializer.Deserialize<Menuitem>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                MenuItemName = menuItem?.Name;
            }
            else
            {
                return RedirectToPage("/MyOrders");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || Rating == null || string.IsNullOrEmpty(ReviewText))
            {
                Message = "Пожалуйста, заполните все поля.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var review = new
            {
                MenuitemId = MenuItemId,
                Rating,
                ReviewText
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(review), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5243/api/reviews/add", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Message = "Ваш отзыв успешно отправлен!";
                return RedirectToPage("/MyOrders");
            }
            else
            {
                Message = "Не удалось отправить отзыв. Попробуйте позже.";
                return Page();
            }
        }
    }
}
