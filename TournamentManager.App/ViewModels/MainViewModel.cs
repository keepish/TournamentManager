using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
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
        private readonly SecureStorage _secureStorage;

        [ObservableProperty]
        private UserInfo currentUser;

        [ObservableProperty]
        private object currentView;

        [ObservableProperty]
        private ObservableCollection<MenuItem> menuItems;

        [ObservableProperty]
        private bool isMenuCollapsed = false;

        public MainViewModel(ApiService apiService,IService<TournamentDto> tournamentService, UserInfo user, SecureStorage secureStorage)
        {
            _apiService = apiService;
            _tournamentService = tournamentService;
            _secureStorage = secureStorage;

            CurrentUser = _apiService.GetStoredUser();
            CurrentView = new DashboardView { DataContext = new DashboardViewModel(_apiService, CurrentUser) };

            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            MenuItems = new ObservableCollection<MenuItem>();

            switch (CurrentUser.Role)
            {
                case "Организатор":
                    MenuItems.Add(new MenuItem("Заявки", new RelayCommand(() => Navigate("Applications")), "ClipboardList"));
                    MenuItems.Add(new MenuItem("Пользователи", new RelayCommand(() => Navigate("Users")), "AccountGroup"));
                    MenuItems.Add(new MenuItem("Отчёты", new RelayCommand(() => Navigate("Reports")), "ChartBar"));
                    break;

                case "Судья":
                    MenuItems.Add(new MenuItem("Мои площадки", new RelayCommand(() => Navigate("MyMatches")), "Stadium"));
                    MenuItems.Add(new MenuItem("Судьи", new RelayCommand(() => Navigate("Judges")), "Gavel"));
                    break;
            }
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            CurrentView = viewName switch
            {
                "Dashboard" => new DashboardView(),
                "Tournaments" => new TournamentsView { DataContext = new TournamentsViewModel(_apiService, _tournamentService, this) },
                "CreateTournament" => new TournamentCreationView { DataContext = new TournamentCreationViewModel(_tournamentService, this) },
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
        private void ToggleMenu()
        {
            IsMenuCollapsed = !IsMenuCollapsed;
        }

        [RelayCommand]
        private void Logout()
        {
            _apiService.ClearToken();

            var loginWindow = new LoginWindow();

            var secureStorage = new SecureStorage();
            var apiService = new ApiService(secureStorage);
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7074/api/Tournaments/");
            var tournamentService = new TournamentService(httpClient);

            loginWindow.DataContext = new LoginViewModel(apiService, tournamentService, secureStorage);

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

        public void NavigateToEditTournament(TournamentDto tournament)
        {
            if (tournament == null)
                return;

            CurrentView = new TournamentEditionView
            {
                DataContext = new TournamentEditionViewModel(tournament, _tournamentService, this)
            };
        }

        public void NavigateToTournamentDetails(TournamentDto tournament)
        {
            if (tournament == null)
                return;

            CurrentView = new TournamentDetailsView
            {
                DataContext = new TournamentDetailsViewModel(tournament, _apiService, this)
            };
        }
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public IRelayCommand Command { get; set; }

        public MenuItem(string title, IRelayCommand command, string icon)
        {
            Title = title;
            Command = command;
            Icon = icon;
        }
    }
}
