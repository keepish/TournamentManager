using TournamentManager.Core.DTOs.Participants;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.DTOs.Categories
{
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(this Category category)
        {
            if (category is null)
                throw new ArgumentNullException(nameof(category));

            return new CategoryDto
            {
                Id = category.Id,
                MinWeight = category.MinWeight,
                MaxWeight = category.MaxWeight,
                MinAge = category.MinAge,
                MaxAge = category.MaxAge
            };
        }
    }
}
