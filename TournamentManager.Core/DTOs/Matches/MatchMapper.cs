using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.Matches
{
    public static class MatchMapper
    {
        public static MatchDto ToDto(this Match match)
        {
            if (match is null)
                throw new ArgumentNullException(nameof(match));

            return new MatchDto
            {
                Id = match.Id,
                FirstParticipantId = match.FirstParticipantId,
                SecondParticipantId = match.SecondParticipantId,
                FirstParticipantScore = match.FirstParticipantScore,
                SecondParticipantScore = match.SecondParticipantScore
            };
        }
    }
}
