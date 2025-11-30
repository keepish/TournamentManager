namespace TournamentManager.Core.Models;

public partial class TournamentCategory
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int CategoryId { get; set; }

    public int JudgeId { get; set; }

    public int SitesNumber { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User Judge { get; set; } = null!;

    public virtual ICollection<ParticipantTournamentCategory> ParticipantTournamentCategories { get; set; } = new List<ParticipantTournamentCategory>();

    public virtual Tournament Tournament { get; set; } = null!;
}
