using System.ComponentModel.DataAnnotations;

namespace TournamentManager.Core.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Login {  get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Surname { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Patronymic { get; set; }

        [StringLength(1000)]
        public string? Password { get; set; }

        public string FullName => Patronymic != null ? $"{Surname} {Name} {Patronymic}" : $"{Surname} {Name}";
    }
}
