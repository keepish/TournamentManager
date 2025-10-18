using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TournamentManager.App.ViewModels;
using TournamentManager.Core.Services;

namespace TournamentManager.Client
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly Core.Services.Navigation.NavigationService navigationService;

        public LoginWindow()
        {
            InitializeComponent();

            var apiService = new ApiService();
            DataContext = new LoginViewModel(apiService);

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
