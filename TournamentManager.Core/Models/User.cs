using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Хеш пароля обязателен")]
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
        public string? Patronymic { get; set; }

        public string FullName => Patronymic != null ? $"{LastName} {FirstName} {Patronymic}" : $"{LastName} {FirstName}";

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(25)]
        public string? BeltLevel { get; set; }

        [StringLength(50)]
        public string? SportsRank { get; set; }

        [StringLength(100)]
        public string? Coach {  get; set; }

        [StringLength(100)] 
        public string? Settlement { get; set; }

        public DateTime? Birthday { get; set; }

        [StringLength(10)]
        public string? PassportDetails { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<Tournament> OrganizedTournaments { get; set; } = new();
    }
}
