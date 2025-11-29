using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.ParticipantTournamentCategories
{
    public static class ParticipantTournamentCategoryMapper
    {
        public static ParticipantTournamentCategoryDto ToDto(this ParticipantTournamentCategory participantTournamentCategory)
        { 
            if (participantTournamentCategory is null)
                throw new ArgumentNullException(nameof(participantTournamentCategory));

            return new ParticipantTournamentCategoryDto
            {
                Id = participantTournamentCategory.Id,
                TournamentCategoryId = participantTournamentCategory.TournamentCategoryId,
                ParticipantId = participantTournamentCategory.ParticipantId
            };
        }
    }
}
