using System.Net.Http.Json;
using TournamentManager.Core.DTOs.TournamentCategories;

namespace TournamentManager.Core.Services
{
    public class TournamentCategoryService : IService<TournamentCategoryDto>
    {
        private readonly HttpClient _client;

        public TournamentCategoryService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<TournamentCategoryDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<TournamentCategoryDto?>?>("");

        public async Task<TournamentCategoryDto?> GetAsync(int id)
            => await _client.GetFromJsonAsync<TournamentCategoryDto?>($"{id}");

        public async Task UpdateAsync(TournamentCategoryDto tournamentCategoryDto)
        {
            HttpResponseMessage response =
                await _client.PutAsJsonAsync($"{tournamentCategoryDto.Id}", tournamentCategoryDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddAsync(TournamentCategoryDto tournamentCategoryDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync("", tournamentCategoryDto);
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
