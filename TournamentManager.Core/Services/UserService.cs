using System.Collections.Generic;
using System.Net.Http.Json;
using TournamentManager.Core.DTOs.Users;

namespace TournamentManager.Core.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _client;

        public UserService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<UserDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<UserDto?>?>("api/users");

        public async Task<UserDto?> GetByIdAsync(int id)
            => await _client.GetFromJsonAsync<UserDto?>($"api/users/{id}");

        public async Task<List<UserDto?>?> GetJudgesAsync()
            => await _client.GetFromJsonAsync<List<UserDto?>?>("api/users/judges");

        public async Task<List<UserDto?>?> GetOrganizersAsync()
            => await _client.GetFromJsonAsync<List<UserDto?>?>("api/users/organizers");

        public async Task<UserDto?> CreateUserAsync(UserDto userDto)
        {
            var response =
                await _client.PostAsJsonAsync("api/users", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        public async Task<bool> UpdateUserAsync(int id, UserDto userDto)
        {
            try
            {
                var response =
                    await _client.PutAsJsonAsync($"api/users/{id}", userDto);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var response =
                    await _client.DeleteAsync($"api/users/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
