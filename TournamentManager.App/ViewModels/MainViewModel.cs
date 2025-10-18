using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.App;
using TournamentManager.App.ViewModels;
using TournamentManager.Client.Views;
using TournamentManager.Core.Models.Responses;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private UserInfo _currentUser;

        [ObservableProperty]
        private object _currentView;

        [ObservableProperty]
        private ObservableCollection<MenuItem> _menuItems;

        public MainViewModel(ApiService apiService, UserInfo user)
        {
            _apiService = apiService;
            CurrentUser = user;

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
                "Tournaments" => new TournamentsView(),
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
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = new LoginViewModel(_apiService);
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
