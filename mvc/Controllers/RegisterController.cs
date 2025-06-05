using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;

namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:5185/api/Auth/register", dto);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Rejestracja nie powiodła się.";
                    return View(dto);
                }

                // Przekierowanie do logowania
                return RedirectToAction("Login1", "Authorization");
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Nie można połączyć się z serwerem.";
                return View(dto);
            }
        }
    }
}
