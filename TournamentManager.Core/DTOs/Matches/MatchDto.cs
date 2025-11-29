namespace TournamentManager.Core.DTOs.Matches
{
    public class MatchDto
    {
        public int Id { get; set; }

        public int FirstParticipantId { get; set; }

        public int? SecondParticipantId { get; set; }

        public int FirstParticipantScore { get; set; }

        public int SecondParticipantScore { get; set; }
    }
}
