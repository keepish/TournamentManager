using System;
using System.Collections.Generic;

namespace TournamentManager.Core.Models;

public partial class Match
{
    public int Id { get; set; }

    public int FirstParticipantId { get; set; }

    public int? SecondParticipantId { get; set; }

    public int FirstParticipantScore { get; set; }

    public int SecondParticipantScore { get; set; }

    public virtual ParticipantTournamentCategory FirstParticipant { get; set; } = null!;

    public virtual ParticipantTournamentCategory? SecondParticipant { get; set; }
}
