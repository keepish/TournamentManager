using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.Tournaments
{
    public static class TournamentMapper
    {
        public static TournamentDto ToDto(this Tournament tournament)
        {
            if (tournament is null)
                throw new ArgumentNullException(nameof(tournament));

            return new TournamentDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Description = tournament.Description,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                OrganizerId = tournament.OrganizerId,
                Address = tournament.Address,
            };
        }
    }
}
