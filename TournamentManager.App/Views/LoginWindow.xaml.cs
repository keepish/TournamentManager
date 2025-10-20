using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using TournamentManager.Client.ViewModels;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var apiService = new ApiService();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");

            var tournamentService = new TournamentService(httpClient);

            DataContext = new LoginViewModel(apiService, tournamentService);

            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                }
            };
        }
    }
}
