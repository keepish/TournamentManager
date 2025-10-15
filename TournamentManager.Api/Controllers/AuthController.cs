using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TournamentManager.Core;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController (AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Name == request.Name);

                if (user is null)
                    return Unauthorized(new { message = "Пользователь не найден" });

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Неверный пароль" });

                var token = GenerateJwtToken(user);

                return Ok (new {
                    Token = token,
                    User = new
                    {
                        user.Id,
                        user.Name,
                        Role = user.Role.Name,
                        user.FullName,
                        user.FirstName,
                        user.LastName,
                        user.Patronymic
                    }
                });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = $"Ошибка сервера: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Name == request.Name))
                    return BadRequest(new { message = "Пользователь с таким именем уже существует" });

                var participantRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Участник");
                if (participantRole is null)
                    return BadRequest(new { message = "Роль Participant не найдена в системе" });
                
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    Name = request.Name,
                    PasswordHash = passwordHash,
                    RoleId = participantRole.Id,
                    LastName = request.LastName,
                    FirstName = request.FirstName,
                    Patronymic = request.Patronymic,
                    Email = request.Email,
                    Settlement = request.Settlement,
                    Birthday = request.Birthday,
                    BeltLevel = request.BeltLevel,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Пользователь успешно зарегистрирован" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка регистрации: {ex.Message}" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.Name),
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
    }

    public class LoginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
        public string? Email { get; set; }
        public string? Settlement { get; set; }
        public DateTime? Birthday { get; set; }
        public string? BeltLevel { get; set; }
    }
}
