using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;
using TournamentManager.Client.Views;
using System.Windows;

namespace TournamentManager.Client.ViewModels
{
    public partial class GuestTournamentsViewModel : ObservableObject
    {
        private readonly IService<TournamentDto> _tournamentService;

        [ObservableProperty]
        private ObservableCollection<TournamentDto> tournaments = new();

        public GuestTournamentsViewModel(IService<TournamentDto> tournamentService)
        {
            _tournamentService = tournamentService;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var items = await _tournamentService.GetAllAsync();
                Tournaments = new ObservableCollection<TournamentDto>(items);
            }
            catch
            {
                // ignore errors in guest view
            }
        }

        [RelayCommand]
        private void OpenLogin()
        {
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is TournamentsWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
