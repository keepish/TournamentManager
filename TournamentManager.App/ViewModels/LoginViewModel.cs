using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Windows;
using TournamentManager.Core.Models.Responses;
using TournamentManager.Core.Services;

namespace TournamentManager.App.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isLoading = false;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Введите имя пользователя и пароль");
                return;
            }

            IsLoading = true;

            try
            {
                var loginData = new
                {
                    name = Username,
                    password = Password
                };

                var result = await _apiService.PostAsync<LoginResult>("/api/Auth/login", loginData);

                _apiService.SetToken(result.Token);

                Application.Current.Properties["User"] = result.User;
                Application.Current.Properties["Token"] = result.Token;

                MessageBox.Show($"Добро пожаловать, {result.User.FullName}!", "Успешный вход");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
