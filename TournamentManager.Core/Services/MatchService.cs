using System.Net.Http.Json;
using TournamentManager.Core.DTOs.Matches;

namespace TournamentManager.Core.Services
{
    public class MatchService : IService<MatchDto>
    {
        private readonly HttpClient _client;

        public MatchService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<MatchDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<MatchDto?>?>("");

        public async Task<MatchDto?> GetAsync(int id)
            => await _client.GetFromJsonAsync<MatchDto?>($"{id}");

        public async Task UpdateAsync(MatchDto matchDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync($"{matchDto.Id}", matchDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddAsync(MatchDto matchDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync("", matchDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            HttpResponseMessage response =
                await _client.DeleteAsync($"{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
