using System;
using System.Collections.Generic;

namespace TournamentManager.Core.Models;

public partial class ParticipantTournamentCategory
{
    public int Id { get; set; }

    public int IdTournamentCategory { get; set; }

    public int IdParticipant { get; set; }

    public virtual Participant IdParticipantNavigation { get; set; } = null!;

    public virtual TournamentCategory IdTournamentCategoryNavigation { get; set; } = null!;

    public virtual ICollection<Match> MatchFirstParticipants { get; set; } = new List<Match>();

    public virtual ICollection<Match> MatchSecondParticipants { get; set; } = new List<Match>();
}
