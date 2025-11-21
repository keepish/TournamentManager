using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class TournamentCreationViewModel : ObservableObject
    {
        private readonly IService<TournamentDto> _tournamentService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private TournamentDto tournament = new();

        [ObservableProperty]
        private bool isLoading;

        public ObservableCollection<string> StatusOptions { get; } = new()
        {
            "Предстоящий",
            "Активный",
            "Завершенный"
        };

        public TournamentCreationViewModel(IService<TournamentDto> tournamentService, MainViewModel mainViewModel)
        {
            _tournamentService = tournamentService;
            _mainViewModel = mainViewModel;

            Tournament.StartDate = DateTime.Today;
            Tournament.EndDate = DateTime.Today.AddDays(1);

            Tournament.OrganizerId = _mainViewModel.CurrentUser.Id;
        }

        [RelayCommand]
        private async Task CreateTournament()
        {
            if (!ValidateTournament())
                return;

            IsLoading = true;

            try
            {
                await _tournamentService.AddAsync(Tournament);

                MessageBox.Show("Турнир успешно создан!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                _mainViewModel.NavigateCommand.Execute("Tournaments");
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка при создании турнира: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _mainViewModel.NavigateCommand.Execute("Tournaments");
        }

        private bool ValidateTournament()
        {
            if (string.IsNullOrWhiteSpace(Tournament.Name))
            {
                MessageBox.Show("Введите название турнира", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Tournament.StartDate >= Tournament.EndDate)
            {
                MessageBox.Show("Дата окончания должна быть позже даты начала",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Tournament.Address))
            {
                MessageBox.Show("Введите адрес проведения турнира", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
