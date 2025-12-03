using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TournamentManager.Core.DTOs.Users;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Core.Models.Requests.LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Login == request.Login);

                if (user is null)
                    return Unauthorized(new { message = "Пользователь не найден" });

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Неверный пароль" });

                var (role, isOrganizer) = await DetermineUserRoleAsync(user.Id);
                var token = GenerateJwtToken(user, role);

                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        user.Id,
                        user.Login,
                        Role = role,
                        IsOrganizer = isOrganizer,
                        user.Name,
                        user.Surname,
                        user.Patronymic,
                        user.FullName
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Login == request.Login))
                    return BadRequest(new { message = "Пользователь с таким именем уже существует" });

                if (string.IsNullOrEmpty(request.Password))
                    return BadRequest(new { message = "Пароль обязателен" });

                var user = new User
                {
                    Login = request.Login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Name = request.Name,
                    Surname = request.Surname,
                    Patronymic = request.Patronymic
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok("Пользователь успешно зарегистрирован");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка регистрации");
            }
        }

        private string GenerateJwtToken(User user, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<(string Role, bool IsOrganizer)> DetermineUserRoleAsync(int userId)
        {
            bool isOrganizer = await _context.Tournaments
                .AnyAsync(t => t.OrganizerId == userId);

            if (isOrganizer)
                return ("Организатор", true);

            bool isJudge = await _context.TournamentCategories
                .AnyAsync(tc => tc.JudgeId == userId);

            if (isJudge)
                return ("Судья", false);

            return ("Гость", false);
        }
    }
}
