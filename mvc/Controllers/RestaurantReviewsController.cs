using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;
using System.Collections.Generic;
using System.Linq;

namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Controllers
{
    public class RestaurantReviewsController : Controller
    {
        private readonly HttpClient _httpClient;

        public RestaurantReviewsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index(int restaurantId)
        {
            if (restaurantId == 0)
            {
                return View("RestaurantReviews");
            }

            var reviews = await _httpClient.GetFromJsonAsync<List<ReviewDto>>(
                $"http://localhost:5185/api/Reviews/{restaurantId}/reviews");

            if (reviews == null || !reviews.Any())
            {
                ViewBag.Error = "Brak opinii dla wybranej restauracji lub restauracja nie istnieje.";
                return View("RestaurantReviews", new List<ReviewDto>());
            }

            ViewBag.RestaurantId = restaurantId;

            return View("RestaurantReviews", reviews);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewsByRestaurantId(int restaurantId)
        {

            if (restaurantId == 0)
            {
                return View("RestaurantReviews");
            }

            var reviews = await _httpClient.GetFromJsonAsync<List<ReviewDto>>(
                $"http://localhost:5185/api/Reviews/{restaurantId}/reviews");

            if (reviews == null || !reviews.Any())
            {
                ViewBag.Error = "Brak opinii dla wybranej restauracji lub restauracja nie istnieje.";
                return View(new List<ReviewDto>());
            }

            ViewBag.RestaurantId = restaurantId;

            return View("RestaurantReviews", reviews);
        }
    }
}
