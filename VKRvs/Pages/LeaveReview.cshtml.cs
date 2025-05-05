using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using VKRvs.Models;

namespace VKRvs.Pages
{
    public class LeaveReviewModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public Review Review { get; set; }

        [FromRoute]
        public int MenuItemId { get; set; }

        public LeaveReviewModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            Review = new Review();
        }

        public void OnGet()
        {
            Review.MenuitemId = MenuItemId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();
            string? token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("http://localhost:5243/api/reviews", Review);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Menu");
            }

            ModelState.AddModelError(string.Empty, "Не удалось оставить отзыв.");
            return Page();
        }
    }

}
