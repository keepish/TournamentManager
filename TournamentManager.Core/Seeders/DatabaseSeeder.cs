using BCrypt.Net;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Seeders
{
    public static class DatabaseSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var organizer = new User
                {
                    Login = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FirstName = "Алексей",
                    LastName = "Колосов",
                    RoleId = 1,
                    Email = "admin@taekwondo.ru"
                };

                var judge = new User
                {
                    Login = "judge",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("judge"),
                    FirstName = "Матвей",
                    LastName = "Филоненко",
                    RoleId = 2,
                    Email = "judge@taekwondo.ru"
                };

                context.Users.Add(organizer);
                context.Users.Add(judge);
                context.SaveChanges();

                var tournament = new Tournament
                {
                    Name = "Первый турнир по тхэквондо",
                    Description = "Тестовый турнир для проверки системы",
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(8),
                    Address = "г. Архангельск, ул. Спортивная, 1",
                    Status = "Upcoming",
                    OrganizerId = organizer.Id
                };

                context.Tournaments.Add(tournament);
                context.SaveChanges();
            }
        }
    }
}
