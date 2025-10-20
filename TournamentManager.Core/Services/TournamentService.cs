using System.Net.Http.Json;
using TournamentManager.Core.DTOs.Tournaments;

namespace TournamentManager.Core.Services
{
    public class TournamentService : IService<TournamentDto>
    {
        private readonly HttpClient _client;
        private readonly string _url = "https://localhost:7074/api/Tournaments/";

        public TournamentService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<TournamentDto?>?> GetAllAsync()
            => await _client.GetFromJsonAsync<List<TournamentDto?>?>("");

        public async Task<TournamentDto?> GetAsync(int id)
            => await _client.GetFromJsonAsync<TournamentDto?>($"{id}");

        public async Task UpdateAsync(TournamentDto tournamentDto)
        {
            HttpResponseMessage response =
                await _client.PutAsJsonAsync($"{tournamentDto.Id}", tournamentDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddAsync(TournamentDto tournamentDto)
        {
            HttpResponseMessage response =
                await _client.PostAsJsonAsync("", tournamentDto);
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
