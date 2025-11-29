using System;
using System.Collections.Generic;

namespace TournamentManager.Core.Models;

public partial class ParticipantTournamentCategory
{
    public int Id { get; set; }

    public int TournamentCategoryId { get; set; }

    public int ParticipantId { get; set; }

    public virtual ICollection<Match> MatchFirstParticipants { get; set; } = new List<Match>();

    public virtual ICollection<Match> MatchSecondParticipants { get; set; } = new List<Match>();

    public virtual Participant Participant { get; set; } = null!;

    public virtual TournamentCategory TournamentCategory { get; set; } = null!;
}
