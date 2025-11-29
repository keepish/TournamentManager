using System.Net.Http.Json;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.Participants;

namespace TournamentManager.Core.Services
{
    public class ParticipantService : IService<ParticipantDto>
    {
        private readonly HttpClient _client;

        public ParticipantService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<ParticipantDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<ParticipantDto?>?>("");

        public async Task<ParticipantDto?> GetAsync(int id)
            => await _client.GetFromJsonAsync<ParticipantDto?>($"{id}");

        public async Task UpdateAsync(ParticipantDto participantDto)
        {
            HttpResponseMessage response =
                await _client.PutAsJsonAsync($"{participantDto.Id}", participantDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddAsync(ParticipantDto participantDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync("", participantDto);
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
