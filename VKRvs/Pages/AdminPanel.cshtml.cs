using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using VKRvs.Models;

namespace VKRvs.Pages.Admin
{
    public class AdminPanelModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminPanelModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<Customer> Users { get; set; } = new();
        public List<Menuitem> MenuItems { get; set; } = new();

        [BindProperty] public Customer NewUser { get; set; } = new();
        [BindProperty] public Menuitem NewMenuItem { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            Users = await client.GetFromJsonAsync<List<Customer>>("http://localhost:5243/api/admin/users") ?? new();
            MenuItems = await client.GetFromJsonAsync<List<Menuitem>>("http://localhost:5243/api/admin/menu") ?? new();
        }

        public async Task<IActionResult> OnPostAddUserAsync()
        {
            var client = _httpClientFactory.CreateClient();
            await client.PostAsJsonAsync("http://localhost:5243/api/admin/users", NewUser);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync($"http://localhost:5243/api/admin/users/{id}");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddMenuItemAsync()
        {
            var client = _httpClientFactory.CreateClient();
            await client.PostAsJsonAsync("http://localhost:5243/api/admin/menu", NewMenuItem);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMenuItemAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync($"http://localhost:5243/api/admin/menu/{id}");
            return RedirectToPage();
        }
    }
}
