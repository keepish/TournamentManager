namespace TournamentManager.Core.DTOs.Tournaments
{
    public class TournamentRegistrationCategoryResultDto
    {
        public int TournamentCategoryId { get; set; }
        public int CategoryId { get; set; }
        public int Gender { get; set; }
        public int ParticipantsRegistered { get; set; }
        public int MatchesCreated { get; set; }
    }

    public class TournamentRegistrationResultDto
    {
        public int TournamentId { get; set; }
        public List<TournamentRegistrationCategoryResultDto> CategoryResults { get; set; } = new();
        public int TotalParticipantsRegistered => CategoryResults.Sum(r => r.ParticipantsRegistered);
        public int TotalMatchesCreated => CategoryResults.Sum(r => r.MatchesCreated);
    }
}
