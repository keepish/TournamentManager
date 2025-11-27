using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Models.Responses;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IService<TournamentDto> _tournamentService;
        private readonly SecureStorage _secureStorage;
        private readonly IService<CategoryDto> _categoryService;

        [ObservableProperty]
        private string login;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isLoading = false;

        public LoginViewModel(ApiService apiService,
            IService<TournamentDto> tournamentService,
            SecureStorage secureStorage,
            IService<CategoryDto> categoryService)
        {
            _apiService = apiService;
            _tournamentService = tournamentService;
            _secureStorage = secureStorage;
            _categoryService = categoryService;
        }

        [RelayCommand]
        private async Task Authorize()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Введите имя пользователя и пароль");
                return;
            }

            IsLoading = true;

            try
            {
                var loginData = new
                {
                    login = Login,
                    password = Password
                };

                var result = await _apiService.PostAsync<LoginResult>("/api/Auth/login", loginData);

                _apiService.SetToken(result.Token);
                _apiService.SaveUser(result.User);

                _secureStorage.Save("AuthToken", result.Token);

                var userJson = System.Text.Json.JsonSerializer.Serialize(result.User);
                _secureStorage.Save("UserData", userJson);

                var mainWindow = App.ServiceProvider.GetService<MainWindow>();
                var mainViewModel = new MainViewModel(_apiService, _tournamentService, _categoryService, result.User, _secureStorage);

                mainWindow.DataContext = mainViewModel;
                mainWindow.Show();

                CloseLoginWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CloseLoginWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
