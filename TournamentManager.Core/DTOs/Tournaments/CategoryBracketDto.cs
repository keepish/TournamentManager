namespace TournamentManager.Core.DTOs.Tournaments
{
    public class BracketMatchItemDto
    {
        public int MatchId { get; set; }
        public int FirstParticipantTournamentCategoryId { get; set; }
        public int? SecondParticipantTournamentCategoryId { get; set; }
        public string FirstParticipantName { get; set; } = string.Empty;
        public string? SecondParticipantName { get; set; }
        public int FirstParticipantScore { get; set; }
        public int SecondParticipantScore { get; set; }
        public bool IsStarted { get; set; }
        public bool IsFinished { get; set; }
        // New: round and ordering inside round
        public int Round { get; set; } // 1-based round number; 0 if unknown
        public int Order { get; set; } // position within round for layout (0 if unknown)
    }

    public class CategoryBracketDto
    {
        public int TournamentCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryDisplay { get; set; } = string.Empty;
        public List<BracketMatchItemDto> Matches { get; set; } = new();
    }
}
