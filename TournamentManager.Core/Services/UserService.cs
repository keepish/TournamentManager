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
            => await _client.GetFromJsonAsync<List<UserDto?>?>($"");

        public async Task<UserDto?> GetByIdAsync(int id)
            => await _client.GetFromJsonAsync<UserDto?>($"{id}");

        public async Task<List<UserDto?>?> GetJudgesAsync()
            => await _client.GetFromJsonAsync<List<UserDto?>?>($"judges");

        public async Task<List<UserDto?>?> GetOrganizersAsync()
            => await _client.GetFromJsonAsync<List<UserDto?>?>($"organizers");

        public async Task<UserDto?> CreateUserAsync(UserDto userDto)
        {
            var response = await _client.PostAsJsonAsync($"", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<bool> UpdateUserAsync(int id, UserDto userDto)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"{id}", userDto);
                return response.IsSuccessStatusCode;
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
                var response = await _client.DeleteAsync($"{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
