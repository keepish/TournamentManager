using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.DTOs.Users;
using TournamentManager.Core.Models;

namespace TournamentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(u => u.ToDto()).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null)
                return NotFound();

            return user.ToDto();
        }

        [HttpGet("judges")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetJudges()
        {
            var judges = await _context.Users
                .Where(u => _context.TournamentCategories.Any(tc => tc.JudgeId == u.Id))
                .ToListAsync();

            return judges.Select(u => u.ToDto()).ToList();
        }

        [HttpGet("organizers")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetOrganizers()
        {
            var organizers = await _context.Users
                .Where(u => _context.Tournaments.Any(t => t.OrganizerId == u.Id))
                .ToListAsync();

            return organizers.Select(u => u.ToDto()).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser([FromBody] UserDto userDto)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Login == userDto.Login))
                    return BadRequest("Пользователь с таким логином уже существует");

                if (string.IsNullOrEmpty(userDto.Password))
                    return BadRequest("Пароль обязателен при создании пользователя");

                var user = new User
                {
                    Login = userDto.Login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    Name = userDto.Name,
                    Surname = userDto.Surname,
                    Patronymic = userDto.Patronymic
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка создания пользователя");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDto userDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user is null)
                    return NotFound("Пользователь не найден");

                if (await _context.Users.AnyAsync(u => u.Login == userDto.Login && u.Id != id))
                    return BadRequest("Пользователь с таким логином уже существует");

                user.Name = userDto.Name;
                user.Surname = userDto.Surname;
                user.Patronymic = userDto.Patronymic;
                user.Login = userDto.Login;

                if (!string.IsNullOrEmpty(userDto.Password))
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok("Пользователь успешно обновлен");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка обновления пользователя");
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user is null)
                    return NotFound("Пользователь не найден");

                if (await _context.Tournaments.AnyAsync(t => t.OrganizerId == id))
                    return BadRequest("Невозможно удалить пользователя с ролью Организатор");

                if (await _context.TournamentCategories.AnyAsync(tc => tc.JudgeId == id))
                    return BadRequest("Невозможно удалить пользователя, так как он судья в одной из категорий турнира");

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok("Пользователь успешно удален");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка удаления пользователя");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
