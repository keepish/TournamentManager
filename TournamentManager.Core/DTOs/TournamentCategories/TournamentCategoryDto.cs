namespace TournamentManager.Core.DTOs.TournamentCategories
{
    public class TournamentCategoryDto
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }

        public int CategoryId { get; set; }

        public int JudgeId { get; set; }

        public int SitesNumber { get; set; }
    }
}
