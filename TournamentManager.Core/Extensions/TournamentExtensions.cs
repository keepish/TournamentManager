using TournamentManager.Core.Enums;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Extensions
{
    public static class TournamentExtensions
    {
        public static TournamentStatus GetStatus(this Tournament tournament)
        {
            var now = DateTime.Now;

            if (now < tournament.StartDate)
                return TournamentStatus.Upcoming;
            else if (now < tournament.StartDate && now <= tournament.EndDate)
                return TournamentStatus.Active;
            else 
                return TournamentStatus.Completed;
        }

        public static string GetStatusString(this Tournament tournament)
        {
            return tournament.GetStatus() switch
            {
                TournamentStatus.Upcoming => "Предстоящий",
                TournamentStatus.Active => "Активный",
                TournamentStatus.Completed => "Завершенный",
                _ => "Неизвестно"
            };
        }
    }
}
