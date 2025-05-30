using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;

namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthorizationController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Nieprawidłowy model xD.";
                return View(login);
            }

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5185/api/Auth/login", login);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Nieprawidłowy login lub hasło.";
                return View(login);
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            {
                ViewBag.Error = "Błąd podczas logowania.";
                return View(login);
            }

            HttpContext.Session.SetString("JwtToken", authResponse.Token!);
            HttpContext.Session.SetString("UserLogin", authResponse.Login!);
            HttpContext.Session.SetInt32("UserId", authResponse.UserId);
            HttpContext.Session.SetString("IsAdmin", authResponse.IsAdmin?.ToString() ?? "false");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
