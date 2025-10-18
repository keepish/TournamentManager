using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Navigation;
using TournamentManager.App.ViewModels;
using TournamentManager.Client;
using TournamentManager.Client.ViewModels;
using TournamentManager.Core.Services;

namespace TournamentManager.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var loginWindow = new LoginWindow();
            loginWindow.DataContext = ServiceProvider.GetService<LoginViewModel>();
            loginWindow?.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<ApiService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider = null;
            base.OnExit(e);
        }
    }
}
