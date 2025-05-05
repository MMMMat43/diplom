using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using VKRvs.DTO;

namespace VKRvs.Pages
{
    public class AdminLoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string? Message { get; set; }

        public AdminLoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Проверка логина и пароля для администратора
            if (Username == "admin" && Password == "admin")
            {
                // Если логин и пароль верные, перенаправляем на админ панель
                return RedirectToPage("/AdminPanel");
            }
            else
            {
                // Если данные неверные, показываем сообщение об ошибке
                Message = "Неверный логин или пароль для администратора.";
                return Page();
            }
        }
    }
}
