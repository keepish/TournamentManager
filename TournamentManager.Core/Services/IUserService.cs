using TournamentManager.Core.DTOs.Users;

namespace TournamentManager.Core.Services
{
    public interface IUserService
    {
        Task<List<UserDto?>?> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<List<UserDto?>?> GetJudgesAsync();
        Task<List<UserDto?>?> GetOrganizersAsync();
        Task<UserDto?> CreateUserAsync(UserDto userDto);
        Task<bool> UpdateUserAsync(int id, UserDto userDto);
        Task<bool> DeleteUserAsync(int id);
    }
}
