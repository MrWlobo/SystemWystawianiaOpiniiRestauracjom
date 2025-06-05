using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;

namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Controllers
{
    public class AddRestaurantsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AddRestaurantsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("AddRestaurants");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddRestaurantDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Nieprawidłowe dane. Sprawdź formularz.";
                return View("AddRestaurants", dto);
            }

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Brak autoryzacji. Zaloguj się.";
                return View("AddRestaurants", dto);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:5185/api/Restaurants", dto);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Błąd podczas dodawania restauracji.";
                    return View("AddRestaurants", dto);
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Wystąpił problem z połączeniem do serwera.";
                return View("AddRestaurants", dto);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
