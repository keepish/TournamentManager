using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;
using System.Threading.Tasks;
using System.Windows;

namespace TournamentManager.Client.ViewModels
{
    public partial class GuestViewModel : ObservableObject
    {
        private readonly IService<TournamentDto> _tournamentService;

        [ObservableProperty]
        private ObservableCollection<TournamentDto> tournaments = new();

        public Action? CloseAction { get; set; }

        public GuestViewModel(IService<TournamentDto> tournamentService)
        {
            _tournamentService = tournamentService;
            LoadTournamentsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadTournaments()
        {
            try
            {
                var list = await _tournamentService.GetAllAsync();
                Tournaments.Clear();
                if (list != null)
                {
                    foreach (var t in list)
                    {
                        if (t != null)
                            Tournaments.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки турниров: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Enter()
        {
            CloseAction?.Invoke();
        }

        [RelayCommand]
        private void Close()
        {
            CloseAction?.Invoke();
        }
    }
}