using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LoginR;
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

        public async Task<IActionResult> Login1(LoginR.LoginRequest login)
        {
            Console.WriteLine(login.Login1);
            Console.WriteLine(login.Password);

            if (!ModelState.IsValid)
                return View(login);

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5185/api/Auth/login", login);
            Console.WriteLine(response);

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
