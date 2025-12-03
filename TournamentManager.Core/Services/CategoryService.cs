using System.Net.Http.Json;
using TournamentManager.Core.DTOs.Categories;

namespace TournamentManager.Core.Services
{
    public class CategoryService : IService<CategoryDto>
    {
        private readonly HttpClient _client;

        public CategoryService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<CategoryDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<CategoryDto?>?>("");

        public async Task<CategoryDto?> GetAsync(int id)
            => await _client.GetFromJsonAsync<CategoryDto?>($"{id}");

        public async Task UpdateAsync(CategoryDto categoryDto)
        {
            HttpResponseMessage response =
                await _client.PutAsJsonAsync($"{categoryDto.Id}", categoryDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddAsync(CategoryDto categoryDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync("", categoryDto);
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
