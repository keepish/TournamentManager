using CommunityToolkit.Mvvm.ComponentModel;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class TournamentsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public TournamentsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }
    }
}
