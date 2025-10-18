using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using TournamentManager.Client;
using TournamentManager.Client.ViewModels;
using TournamentManager.Core.Models.Responses;
using TournamentManager.Core.Services;

namespace TournamentManager.App.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string login;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isLoading = false;

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
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

                Application.Current.Properties["User"] = result.User;
                Application.Current.Properties["Token"] = result.Token;

                var mainWindow = new MainWindow();
                var mainViewModel = new MainViewModel(_apiService, result.User);
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
