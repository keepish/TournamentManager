
using System.Net.Http.Json;
using TournamentManager.Core.DTOs.ParticipantTournamentCategories;

namespace TournamentManager.Core.Services
{
    public class ParticipantTournamentCategoryService : IService<ParticipantTournamentCategoryDto>
    {
        private readonly HttpClient _client;

        public ParticipantTournamentCategoryService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<ParticipantTournamentCategoryDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<ParticipantTournamentCategoryDto?>?>("");

        public async Task<ParticipantTournamentCategoryDto?> GetAsync(int id)
            => await _client.GetFromJsonAsync<ParticipantTournamentCategoryDto?>($"{id}");

        public async Task UpdateAsync(ParticipantTournamentCategoryDto participantTournamentCategoryDto)
        {
            HttpResponseMessage response =
                await _client.PutAsJsonAsync($"{participantTournamentCategoryDto}", participantTournamentCategoryDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddAsync(ParticipantTournamentCategoryDto participantTournamentCategoryDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync("", participantTournamentCategoryDto);
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
