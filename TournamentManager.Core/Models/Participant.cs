using System;
using System.Collections.Generic;

namespace TournamentManager.Core.Models;

public partial class Participant
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string? Phone { get; set; }

    public ulong Gender { get; set; }

    public DateTime Birthday { get; set; }

    public decimal Weight { get; set; }

    public virtual ICollection<ParticipantTournamentCategory> ParticipantTournamentCategories { get; set; } = new List<ParticipantTournamentCategory>();
}
