using CommunityToolkit.Mvvm.ComponentModel;
using TournamentManager.Core.Models;
using TournamentManager.Core.Models.Responses;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private UserInfo _currentUser;

        [ObservableProperty]
        private bool _isLoadin;

        public DashboardViewModel(ApiService apiService, UserInfo user)
        {
            _apiService = apiService;
            CurrentUser = user;
        }

        public string WelcomeMessage => $"Добро пожаловать, {CurrentUser.FullName}!";

        public string RoleSpecificMessage => CurrentUser.Role switch
        {
            "Организатор" => "Вы можете управлять турнирами, заявками и пользователями",
            "Судья" => "Вы можете судить поединки и управлять результатами",
            "Участник" => "Вы можете подавать заявки и просматривать свои результаты",
            _ => "Добро пожаловать в систему управления соревнованиями"
        };
    }
}
