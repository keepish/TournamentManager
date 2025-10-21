using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
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

            var secureStorage = ServiceProvider.GetService<SecureStorage>();
            var apiService = ServiceProvider.GetService<ApiService>();

            if (apiService.IsAuthenticated())
            {
                var user = apiService.GetStoredUser();
                if (user is not null)
                {
                    var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");
                    var tournamentsService = ServiceProvider.GetService<TournamentService>();

                    var mainWindow = ServiceProvider.GetService<MainWindow>();
                    var mainViewModel = new MainViewModel(apiService, tournamentsService, user, secureStorage);
                    mainWindow.DataContext = mainViewModel;
                    mainWindow.Show();
                    return;
                }
            }

            var loginWindow = ServiceProvider.GetService<LoginWindow>();
            var httpClientForLogin = new HttpClient();
            httpClientForLogin.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");
            var tournamentsServiceForLogin = new TournamentService(httpClientForLogin);

            loginWindow.DataContext = new LoginViewModel(apiService, tournamentsServiceForLogin, secureStorage);
            loginWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<SecureStorage>();
            services.AddSingleton<ApiService>(provider =>
            {
                var secureStorage = provider.GetService<SecureStorage>();
                return new ApiService(secureStorage);
            });

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
