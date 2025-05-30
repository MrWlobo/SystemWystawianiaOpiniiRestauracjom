using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Models;
using DTOs;
using BCrypt.Net;
using Db;

namespace JwtAuthDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ContextDb _context;
        private readonly IConfiguration _configuration;

        public AuthController(ContextDb context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Login == request.Login))
            {
                return Conflict("User with this login already exists.");
            }

            var user = new User
            {
                Login = request.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password), 
                isAdmin = false 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine(request);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == request.Login1);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            
            var tokenHandler = new JwtSecurityTokenHandler();
            // var secret = _configuration["JwtSettings:Secret"];
            string secret = "abcdefghijklmnoprstuwxyz1234567890ABCDEFGH";
            var key = Encoding.UTF8.GetBytes(secret ?? string.Empty);
            // var issuer = _configuration["JwtSettings:Issuer"];
            // var audience = _configuration["JwtSettings:Audience"];
            // var expiresInMinutes = double.Parse(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60");
            var issuer = "YourAppIssuer";
            var audience = "YourAppAudience";
            var expiresInMinutes = 60.0;


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Login ?? string.Empty),
                new Claim(ClaimTypes.Role, user.isAdmin ? "Admin" : "User") 
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new AuthResponse
            {
                Token = tokenString,
                UserId = user.UserId,
                Login = user.Login,
                IsAdmin = user.isAdmin
            });
        }
    }
}