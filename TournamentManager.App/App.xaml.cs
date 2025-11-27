using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Windows;
using TournamentManager.Client;
using TournamentManager.Client.ViewModels;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Categories;
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
                    // Получаем сервисы через DI
                    var tournamentService = ServiceProvider.GetService<IService<TournamentDto>>();
                    var categoryService = ServiceProvider.GetService<IService<CategoryDto>>();

                    var mainWindow = ServiceProvider.GetService<MainWindow>();
                    var mainViewModel = new MainViewModel(apiService, tournamentService, categoryService, user, secureStorage);
                    mainWindow.DataContext = mainViewModel;
                    mainWindow.Show();
                    return;
                }
            }

            // Для логина получаем сервисы через DI
            var loginWindow = ServiceProvider.GetService<LoginWindow>();
            var tournamentServiceForLogin = ServiceProvider.GetService<IService<TournamentDto>>();
            var categoryServiceForLogin = ServiceProvider.GetService<IService<CategoryDto>>();

            loginWindow.DataContext = new LoginViewModel(apiService, tournamentServiceForLogin, secureStorage, categoryServiceForLogin);
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

            // Регистрация TournamentService
            services.AddHttpClient<IService<TournamentDto>, TournamentService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");
            });

            // Регистрация CategoryService
            services.AddHttpClient<IService<CategoryDto>, CategoryService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7074/api/Categories/");
            });

            // Регистрация ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<TournamentsViewModel>();
            services.AddTransient<TournamentCreationViewModel>();
            services.AddTransient<TournamentEditionViewModel>();
            services.AddTransient<TournamentDetailsViewModel>();
            services.AddTransient<CategoriesManagementViewModel>();
            services.AddTransient<DashboardViewModel>();

            // Регистрация Views
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<TournamentsView>();
            services.AddTransient<DashboardView>();
            services.AddTransient<PlaceholderView>();
            services.AddTransient<TournamentCreationView>();
            services.AddTransient<TournamentEditionView>();
            services.AddTransient<TournamentDetailsView>();
            services.AddTransient<CategoriesManagementWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider = null;
            base.OnExit(e);
        }
    }
}