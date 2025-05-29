using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;


namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Controllers
{
    public class CuisineFilterController : Controller
    {
        private readonly HttpClient _httpClient;

        public CuisineFilterController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index(string cuisine)
        {
            var restaurants = await _httpClient.GetFromJsonAsync<List<RestaurantDetailDto>>(
                "http://localhost:5185/api/Restaurants");

            var filtered = restaurants
                .Where(r => r.Cuisine.CuisineName.Equals(cuisine, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View("CuisineFilter", filtered);
        }

        [HttpGet]
        public async Task<IActionResult> FilterByCuisineId(int cuisineId)
        {
            // 1. Pobierz nazwę kuchni na podstawie ID
            var cuisine = await _httpClient.GetFromJsonAsync<CuisineDto>(
                $"http://localhost:5185/api/Cuisines/{cuisineId}");

            if (cuisine == null || string.IsNullOrEmpty(cuisine.CuisineName))
            {
                ViewBag.Error = "Nie znaleziono kuchni o podanym ID.";
                return View(new List<RestaurantDetailDto>());
            }

            // 2. Pobierz wszystkie restauracje
            var allRestaurants = await _httpClient.GetFromJsonAsync<List<RestaurantDetailDto>>(
                $"http://localhost:5185/api/Restaurants");

            // 3. Filtrowanie po nazwie kuchni
            var filtered = allRestaurants
                .Where(r => r.Cuisine?.CuisineName?.Equals(cuisine.CuisineName, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            ViewBag.CuisineName = cuisine.CuisineName;

            return View("CuisineFilter", filtered);
        }
    }
}
