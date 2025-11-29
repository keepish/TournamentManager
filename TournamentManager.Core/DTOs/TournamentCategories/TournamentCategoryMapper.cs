using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.TournamentCategories
{
    public static class TournamentCategoryMapper
    {
        public static TournamentCategoryDto ToDto(this TournamentCategory tournamentCategory)
        {
            if (tournamentCategory is null)
                throw new ArgumentNullException(nameof(tournamentCategory));

            return new TournamentCategoryDto
            {
                Id = tournamentCategory.Id,
                TournamentId = tournamentCategory.TournamentId,
                CategoryId = tournamentCategory.CategoryId,
                JudgeId = tournamentCategory.JudgeId,
                SitesNumber = tournamentCategory.SitesNumber
            };
        }
    }
}
