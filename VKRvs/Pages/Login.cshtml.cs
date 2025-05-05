using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using VKRvs.DTO;

namespace VKRvs.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public LoginDto LoginData { get; set; } = new();

        public string? Message { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Введите корректные данные.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonSerializer.Serialize(LoginData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5243/api/auth/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsStringAsync();
                Message = "Вы успешно вошли в систему!";
                // Здесь можно сохранить токен в localStorage, cookies или другую систему
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                var jsonObject = JsonSerializer.Deserialize<Dictionary<string, string>>(result);

                if (jsonObject != null && jsonObject.ContainsKey("token"))
                {
                    string token = jsonObject["token"];

                    // Сохранение токена в сессию
                    HttpContext.Session.SetString("JwtToken", token);
                }
                else
                {
                    Console.WriteLine("Токен не найден в ответе.");
                }

                return RedirectToPage("/Menu");
            }
            else
            {
                Message = "Ошибка авторизации. Проверьте данные.";
                return Page();
            }
        }
    }
}
