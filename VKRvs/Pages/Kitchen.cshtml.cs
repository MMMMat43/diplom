using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class KitchenModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<Order> Orders { get; set; }

        public KitchenModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            Orders = new();
        }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5243/api/kitchen/orders");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int itemId, string newStatus)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent($"\"{newStatus}\"", Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"http://localhost:5243/api/kitchen/orderitem/{itemId}/status", content);

            return RedirectToPage();
        }
    }
}
