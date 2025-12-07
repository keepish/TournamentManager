using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using TournamentManager.Client.ViewModels;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.Participants;
using TournamentManager.Core.DTOs.TournamentCategories;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;

namespace TournamentManager.Client
{
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
                    var tournamentService = ServiceProvider.GetService<IService<TournamentDto>>();
                    var categoryService = ServiceProvider.GetService<IService<CategoryDto>>();
                    var tournamentCategoryService = ServiceProvider.GetService<ITournamentCategoryService>();
                    var userService = ServiceProvider.GetService<IUserService>();
                    var participantService = ServiceProvider.GetService<IParticipantService>();

                    var mainWindow = ServiceProvider.GetService<MainWindow>();
                    var mainViewModel = new MainViewModel(apiService, tournamentService, categoryService,
                        user, secureStorage, tournamentCategoryService, userService, participantService);
                    mainWindow.DataContext = mainViewModel;
                    mainWindow.Show();
                    return;
                }
            }

            var loginWindow = ServiceProvider.GetService<LoginWindow>();
            var tournamentServiceForLogin = ServiceProvider.GetService<IService<TournamentDto>>();
            var categoryServiceForLogin = ServiceProvider.GetService<IService<CategoryDto>>();
            var tournamentCategoryServiceForLogin = ServiceProvider.GetService<ITournamentCategoryService>();
            var userServiceForLogin = ServiceProvider.GetService<IUserService>();
            var participantServiceForLogin = ServiceProvider.GetService<IParticipantService>();

            loginWindow.DataContext = new LoginViewModel(apiService, tournamentServiceForLogin, secureStorage, categoryServiceForLogin,
                tournamentCategoryServiceForLogin, userServiceForLogin, participantServiceForLogin);
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

            services.AddHttpClient<IService<TournamentDto>, TournamentService>((provider, client) =>
            {
                client.BaseAddress = new Uri("http://localhost:7074/api/Tournaments/");
            });

            services.AddHttpClient<IService<CategoryDto>, CategoryService>((provider, client) =>
            {
                client.BaseAddress = new Uri("http://localhost:7074/api/Categories/");
            });

            services.AddHttpClient<ITournamentCategoryService, TournamentCategoryService>((provider, client) =>
            {
                client.BaseAddress = new Uri("http://localhost:7074/api/TournamentCategories/");
            });

            services.AddHttpClient<IUserService, UserService>((provider, client) =>
            {
                client.BaseAddress = new Uri("http://localhost:7074/api/Users/");
            });

            services.AddHttpClient<IParticipantService, ParticipantService>((provider, client) =>
            {
                client.BaseAddress = new Uri("http://localhost:7074/api/Participants/");
            });

            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<TournamentsViewModel>();
            services.AddTransient<TournamentCreationViewModel>();
            services.AddTransient<TournamentEditionViewModel>();
            services.AddTransient<TournamentDetailsViewModel>();
            services.AddTransient<CategoriesManagementViewModel>();
            services.AddTransient<DashboardViewModel>();

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