using System.Windows;
using TournamentManager.App.ViewModels;
using TournamentManager.Core.Services;

namespace TournamentManager.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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