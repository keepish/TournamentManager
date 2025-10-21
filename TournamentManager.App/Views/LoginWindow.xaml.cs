using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using TournamentManager.Client.ViewModels;

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

            DataContext = App.ServiceProvider.GetService<LoginViewModel>();

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
