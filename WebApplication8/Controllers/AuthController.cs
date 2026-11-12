using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly List<User> _users = new();
        private static int _nextId = 1;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(IConfiguration config) => _config = config;

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(AuthRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Username and password are required." });

            if (_users.Any(u => u.Username.Equals(req.Username, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Username already taken." });

            var user = new User { Id = _nextId++, Username = req.Username };
            user.Role = (req.Username == "admin") ? "Admin" : "User";
            user.PasswordHash = _hasher.HashPassword(user, req.Password);
            _users.Add(user);

            return StatusCode(201, new { message = "Registered", username = user.Username });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(AuthRequest req)
        {
            var user = _users.FirstOrDefault(u => u.Username == req.Username);
            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid username or password." });

            return Ok(new { token = CreateToken(user), username = user.Username, role = user.Role });
        }

        private string CreateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}