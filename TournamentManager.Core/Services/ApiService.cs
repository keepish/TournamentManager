using System.Net.Http.Headers;
using Newtonsoft.Json;
using TournamentManager.Core.Models.Responses;

namespace TournamentManager.Core.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly SecureStorage _secureStorage;
        private const string BaseUrl = "https://localhost:7074/";

        public ApiService(SecureStorage secureStorage)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _secureStorage = secureStorage;

            LoadToken();
        }

        private void LoadToken()
        {
            var token = _secureStorage.Load("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", token);
            _secureStorage.Save("AuthToken", token);
        }

        public void ClearToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _secureStorage.Remove("AuthToken");
            _secureStorage.Remove("UserData");
        }

        public bool IsAuthenticated()
        {
            return _secureStorage.Contains("AuthToken") && _secureStorage.Contains("UserData");
        }

        public UserInfo GetStoredUser()
        {
            var userJson = _secureStorage.Load("UserData");
            if (!string.IsNullOrEmpty(userJson))
                return JsonConvert.DeserializeObject<UserInfo>(userJson);
            return null;
        }

        public void SaveUser(UserInfo user)
        {
            var userJson = JsonConvert.SerializeObject(user);
            _secureStorage.Save("UserData", userJson);
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        public async Task<T> PutAsync<T>(string endpoing, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoing, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }
    }
}
