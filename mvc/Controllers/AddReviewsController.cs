using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;

namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Controllers
{
    public class AddReviewsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddReviewsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("AddReviews");
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddReviewDto dto)
        {
            var token = HttpContext.Session.GetString("JtwToken");

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Brak autoryzacji. Zaloguj się.";
                return View("AddReviews", dto);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("http://localhost:5185/api/Reviews", dto);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Nie udało się dodać opinii.";
                return View("AddReviews", dto);
            }

            return RedirectToAction("Index", "RestaurantReviews", new { restaurantId = dto.RestaurantId });
        }
    }
}
