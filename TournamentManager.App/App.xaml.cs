using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TournamentManager.Client;
using TournamentManager.Client.ViewModels;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;

namespace TournamentManager.Client
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

            var loginWindow = ServiceProvider.GetService<LoginWindow>();
            loginWindow?.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<ApiService>();

            services.AddHttpClient<TournamentService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");
            });

            services.AddTransient<IService<TournamentDto>, TournamentService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<TournamentsViewModel>();

            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider = null;
            base.OnExit(e);
        }
    }
}
