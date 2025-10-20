using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using TournamentManager.Client;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Models.Responses;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IService<TournamentDto> _tournamentService;

        [ObservableProperty]
        private UserInfo _currentUser;

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private ObservableCollection<MenuItem> _menuItems;

        public MainViewModel(ApiService apiService,IService<TournamentDto> tournamentService, UserInfo user)
        {
            _apiService = apiService;
            _tournamentService = tournamentService;
            CurrentUser = user;

            CurrentView = new TournamentsViewModel(_apiService, _tournamentService);

            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            MenuItems = new ObservableCollection<MenuItem>();

            switch (CurrentUser.Role)
            {
                case "Организатор":
                    MenuItems.Add(new MenuItem("Управление заявками", new RelayCommand(() => Navigate("Applications"))));
                    MenuItems.Add(new MenuItem("Управление пользователями", new RelayCommand(() => Navigate("Users"))));
                    MenuItems.Add(new MenuItem("Отчёты", new RelayCommand(() => Navigate("Reports"))));
                    break;

                case "Судья":
                    MenuItems.Add(new MenuItem("Мои площадки", new RelayCommand(() => Navigate("MyMatches"))));
                    MenuItems.Add(new MenuItem("Судьи", new RelayCommand(() => Navigate("Judges"))));
                    break;

                case "Участник":
                    MenuItems.Add(new MenuItem("Мои заявки", new RelayCommand(() => Navigate("MyApplications"))));
                    MenuItems.Add(new MenuItem("Мои результаты", new RelayCommand(() => Navigate("MyResults"))));
                    break;
            }
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            CurrentView = viewName switch
            {
                "Dashboard" => new DashboardView(),
                "Tournaments" => new TournamentsView { DataContext = new TournamentsViewModel(_apiService, _tournamentService) },
                "Applications" => new PlaceholderView("Заявки"),
                "Users" => new PlaceholderView("Пользователи"),
                "Reports" => new PlaceholderView("Отчеты"),
                "MyMatches" => new PlaceholderView("Мои площадки"),
                "Judges" => new PlaceholderView("Судьи"),
                "MyApplications" => new PlaceholderView("Мои заявки"),
                "MyResults" => new PlaceholderView("Мои результаты"),
                _ => new DashboardView()
            };
        }

        [RelayCommand]
        private void Logout()
        {
            _apiService.ClearToken();
            Application.Current.Properties["User"] = null;
            Application.Current.Properties["Token"] = null;

            var loginWindow = new LoginWindow();

            var apiService = new ApiService();
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");
            var tournamentService = new TournamentService(httpClient);
            loginWindow.DataContext = new LoginViewModel(apiService, tournamentService);

            loginWindow.Show();

            CloseMainWindow();
        }

        private void CloseMainWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public IRelayCommand Command { get; set; }

        public MenuItem(string title, IRelayCommand command)
        {
            Title = title;
            Command = command;
        }
    }
}
