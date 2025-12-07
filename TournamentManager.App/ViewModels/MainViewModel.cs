using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.TournamentCategories;
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
        private readonly IService<CategoryDto> _categoryService;
        private readonly ITournamentCategoryService _tournamentCategoryService;
        private readonly IUserService _userService;
        private readonly IParticipantService _participantService;

        [ObservableProperty]
        private UserInfo currentUser;

        [ObservableProperty]
        private object currentView;

        [ObservableProperty]
        private ObservableCollection<MenuItem> menuItems;

        [ObservableProperty]
        private bool isMenuCollapsed = false;

        public MainViewModel(ApiService apiService,IService<TournamentDto> tournamentService,
            IService<CategoryDto> categoryService, UserInfo user, SecureStorage secureStorage, 
            ITournamentCategoryService tournamentCategoryService, IUserService userService,
            IParticipantService participantService)
        {
            _apiService = apiService;
            _tournamentService = tournamentService;
            _secureStorage = secureStorage;
            _categoryService = categoryService;
            _tournamentCategoryService = tournamentCategoryService;
            _userService = userService;
            _participantService = participantService;

            CurrentUser = _apiService.GetStoredUser();
            CurrentView = new DashboardView { DataContext = new DashboardViewModel(_apiService, CurrentUser) };

            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            MenuItems = new ObservableCollection<MenuItem>();

            if (CurrentUser.IsOrganizer)
            {
                MenuItems.Add(new MenuItem("Заявки", new RelayCommand(() => Navigate("Applications")), "ClipboardList"));
                MenuItems.Add(new MenuItem("Пользователи", new RelayCommand(() => Navigate("Users")), "AccountGroup"));
                MenuItems.Add(new MenuItem("Отчёты", new RelayCommand(() => Navigate("Reports")), "ChartBar"));
            }
            else
            {
                MenuItems.Add(new MenuItem("Мои площадки", new RelayCommand(() => Navigate("MyMatches")), "Stadium"));
                MenuItems.Add(new MenuItem("Судьи", new RelayCommand(() => Navigate("Judges")), "Gavel"));
            }
        }

        [RelayCommand]
        private void Navigate(string viewName)
        {
            if (viewName == "CreateTournament" && !CurrentUser.IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут управлять турнирами.", "Ошибка доступа");
                return;
            }

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

            var loginWindow = App.ServiceProvider.GetService<LoginWindow>();
            var tournamentService = App.ServiceProvider.GetService<IService<TournamentDto>>();
            var categoryService = App.ServiceProvider.GetService<IService<CategoryDto>>();
            var secureStorage = App.ServiceProvider.GetService<SecureStorage>();
            var apiService = App.ServiceProvider.GetService<ApiService>();
            var tournamentCategoryService = App.ServiceProvider.GetService<ITournamentCategoryService>();
            var userService = App.ServiceProvider.GetService<IUserService>();
            var participantService = App.ServiceProvider.GetService<IParticipantService>();

            loginWindow.DataContext = new LoginViewModel(apiService, tournamentService, secureStorage, 
                categoryService, tournamentCategoryService, userService, participantService);

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

            if (!CurrentUser.IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут редактировать турниры.", "Ошибка доступа");
                return;
            }

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
                DataContext = new TournamentDetailsViewModel(tournament, _apiService, this, _categoryService,
                    _tournamentCategoryService, _userService, _participantService)
            };
        }

        public void NavigateToBrackets(TournamentDto tournament)
        {
            CurrentView = new BracketsView
            {
                DataContext = new BracketsViewModel(_apiService, tournament, CurrentUser.IsOrganizer, this)
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
