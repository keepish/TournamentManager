using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.TournamentCategories;

namespace TournamentManager.Core.Services
{
    public interface ITournamentCategoryService : IService<TournamentCategoryDto>
    {
        Task<List<TournamentCategoryDto?>?> GetByTournamentIdAsync(int tournamentId);
        Task<List<CategoryDto?>?> GetCategoriesByTournamentIdAsync(int tournamentId);
        Task<bool> AttachCategoryToTournamentAsync(int tournamentId, int categoryId, int judgeId, int sitesNumber);
        Task<bool> DetachCategoryFromTournamentAsync(int tournamentId, int categoryId);
    }
}
