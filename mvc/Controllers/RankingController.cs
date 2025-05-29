using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;
using SystemWystawianiaOpiniiRestauracjom.Mvc.Models;

public class RankingController : Controller
{
    private readonly HttpClient _httpClient;

    public RankingController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        var restaurants = await _httpClient.GetFromJsonAsync<List<RestaurantDetailDto>>("http://localhost:5185/api/Restaurants");

        var rankedRestaurants = new List<RestaurantRankingModel>();

        foreach (var restaurant in restaurants)
        {
            var reviews = await _httpClient.GetFromJsonAsync<List<ReviewDto>>(
                $"http://localhost:5185/api/Reviews/{restaurant.RestaurantId}/reviews");

            var average = reviews.Any() ? reviews.Average(r => r.Stars) : 0;

            rankedRestaurants.Add(new RestaurantRankingModel
            {
                RestaurantName = restaurant.RestaurantName,
                AverageRating = (double)(reviews.Average(r => (double?)r.Stars) ?? 0)
            });
        }

        var sorted = rankedRestaurants.OrderByDescending(r => r.AverageRating).ToList();

        return View("Ranking", sorted);
    }
}
