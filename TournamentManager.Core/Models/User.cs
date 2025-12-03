namespace TournamentManager.Core.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<TournamentCategory> TournamentCategories { get; set; } = new List<TournamentCategory>();

    public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();

    public string FullName => Patronymic != null ? $"{Surname} {Name} {Patronymic}" : $"{Surname} {Name}";
}
