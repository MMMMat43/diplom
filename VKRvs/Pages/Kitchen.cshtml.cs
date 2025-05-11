using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class KitchenModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<Order> Orders { get; set; } = new();
        public List<Ingredient> Ingredients { get; set; } = new();

        public KitchenModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            await LoadOrders();
            await LoadIngredients();
        }

        private async Task LoadOrders()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5243/api/kitchen/orders");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        private async Task LoadIngredients()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5243/api/ingredients");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Ingredients = JsonSerializer.Deserialize<List<Ingredient>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int itemId, string newStatus)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent($"\"{newStatus}\"", Encoding.UTF8, "application/json");
            await client.PutAsync($"http://localhost:5243/api/kitchen/orderitem/{itemId}/status", content);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddIngredientAsync(string Name, string Unit, decimal Quantity)
        {
            var ingredient = new Ingredient { Name = Name, Unit = Unit, QuantityInStock = Quantity };
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(ingredient), Encoding.UTF8, "application/json");
            await client.PostAsync("http://localhost:5243/api/ingredients", content);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteIngredientAsync(int Id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync($"http://localhost:5243/api/ingredients/{Id}");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostIncreaseAsync(int Id, decimal Amount)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(Amount), Encoding.UTF8, "application/json");
            await client.PutAsync($"http://localhost:5243/api/ingredients/{Id}/increase", content);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDecreaseAsync(int Id, decimal Amount)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(Amount), Encoding.UTF8, "application/json");
            await client.PutAsync($"http://localhost:5243/api/ingredients/{Id}/decrease", content);
            return RedirectToPage();
        }
    }
}
