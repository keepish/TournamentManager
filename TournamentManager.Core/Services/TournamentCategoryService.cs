using System.Collections.Generic;
using System.Net.Http.Json;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.TournamentCategories;

namespace TournamentManager.Core.Services
{
    public class TournamentCategoryService : ITournamentCategoryService
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

        public async Task<List<TournamentCategoryDto?>?> GetByTournamentIdAsync(int tournamentId)
            => await _client.GetFromJsonAsync<List<TournamentCategoryDto?>?>($"tournament/{tournamentId}");

        public async Task<List<CategoryDto?>?> GetCategoriesByTournamentIdAsync(int tournamentId)
            => await _client.GetFromJsonAsync<List<CategoryDto?>?>($"tournament/{tournamentId}/categories");

        public async Task<bool> AttachCategoryToTournamentAsync(int tournamentId, int categoryId, int judgeId, int sitesNumber)
        {
            try
            {
                var request = new {judgeId, sitesNumber};
                var response = await _client.PostAsJsonAsync($"tournament/{tournamentId}/category/{categoryId}", request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DetachCategoryFromTournamentAsync(int tournamentId, int categoryId)
        {
            try
            {
                var response = await _client.DeleteAsync($"tournament/{tournamentId}/category/{categoryId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
