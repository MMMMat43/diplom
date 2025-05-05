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
                CartItems = new List<Orderitem>(); // ������ �������
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
                CartItems = new List<Orderitem>(); // ������ �������
            }
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();

            // �������� ����� �� ������
            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // ������ �������, ���� ������������ �� �����������
                return RedirectToPage("/Cart");
            }

            // ���������� ������ �� �������� �������� �� �������
            var response = await client.DeleteAsync($"http://localhost:5243/api/cart/remove/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Cart");
            }

            // ���� �� ������� ������� �������
            return BadRequest("�� ������� ������� �������.");
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
                // ���� ������������ �� �����������, �������������� �� �������� �����
                return RedirectToPage("/Login");
            }

            // ���������� ������ �� ������ ��� ���������� ������
            var response = await client.PostAsync("http://localhost:5243/api/cart", null);

            if (response.IsSuccessStatusCode)
            {
                IsOrderPlaced = true;
                CartItems = new List<Orderitem>();
                TempData["OrderMessage"] = "����� ������� ��������!";
                return RedirectToPage("/Cart");
            }

            // ���� ���������� ������ �� �������
            ModelState.AddModelError(string.Empty, "�� ������� �������� �����. ����������, ���������� �����.");
            return Page();
        }


        public async Task<IActionResult> OnPostClearAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // �������� ����� �� ������
            string? token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // ������ �������, ���� ������������ �� �����������
                return RedirectToPage("/Cart");
            }

            // ���������� ������ �� ������� �������
            var response = await client.DeleteAsync("http://localhost:5243/api/cart/clear");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Cart");
            }



            // ���� �� ������� �������� �������
            return BadRequest("�� ������� �������� �������.");
        }

    }
}
