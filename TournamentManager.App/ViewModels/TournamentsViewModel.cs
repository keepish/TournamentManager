using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Enums;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class TournamentsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IService<TournamentDto> _tournamentService;
        private readonly MainViewModel _mainViewModel;

        private ObservableCollection<TournamentDto> _allTournaments = new();

        [ObservableProperty]
        private ObservableCollection<TournamentDto> _tournaments = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string searchText = string.Empty;

        public ObservableCollection<string> StatusFilters { get; } = new()
        {
            "Все",
            "Предстоящий",
            "Активный",
            "Завершенный"
        };

        [ObservableProperty]
        private string _selectedStatusFilter = "Все";

        public TournamentsViewModel(ApiService apiService,
            IService<TournamentDto> tournamentService,
            MainViewModel mainViewModel)
        {
            _apiService = apiService;
            _tournamentService = tournamentService;
            _mainViewModel = mainViewModel;

            LoadTournaments();
        }

        [RelayCommand]
        private async Task LoadTournaments()
        {
            IsLoading = true;

            try
            {
                var tournaments = await _tournamentService.GetAllAsync();

                if (tournaments is not null)
                {
                    _allTournaments.Clear();
                    foreach (var tournament in tournaments)
                    {
                        if (tournament is not null)
                            _allTournaments.Add(tournament);
                    }
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки турниров: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedStatusFilter = StatusFilters[0];

            ApplyFilters();
        }

        [RelayCommand]
        private void CreateTournament()
        {
            _mainViewModel.NavigateCommand.Execute("CreateTournament");
        }

        [RelayCommand]
        private void EditTournament(TournamentDto tournament)
        {
            if (tournament is null)
            {
                MessageBox.Show("Выберите турнир для редактирования", "Внимание");
                return;
            }

            _mainViewModel.NavigateToEditTournament(tournament);
        }

        [RelayCommand]
        private async Task DeleteTournament(TournamentDto tournament)
        {
            if (tournament is null)
            {
                MessageBox.Show("Выберите турнир для удаления", "Внимание");

                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить турнир \"{tournament.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
                );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _tournamentService.DeleteAsync(tournament.Id);
                    MessageBox.Show("Турнир успешно удален", "Успех");

                    var tournamentToRemove = _allTournaments.FirstOrDefault(t => t.Id == tournament.Id);

                    if (tournamentToRemove != null)
                    {
                        _allTournaments.Remove(tournamentToRemove);
                        ApplyFilters();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления турнира: {ex.Message}", "Ошибка");
                }
            }
        }

        [RelayCommand]
        private void ViewTournamentDetails(TournamentDto tournament)
        {
            if (tournament is null)
            {
                MessageBox.Show("Выберите турнир для просмотра", "Внимание");
                return;
            }

            _mainViewModel.NavigateToTournamentDetails(tournament);
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedStatusFilterChanged(string value)
        {
            ApplyFilters();
        }

        private TournamentStatus GetTournamentStatus(TournamentDto tournament)
        {
            var now = DateTime.Now;

            if (now < tournament.StartDate)
                return TournamentStatus.Upcoming;
            else if (now >= tournament.StartDate && now <= tournament.EndDate)
                return TournamentStatus.Active;
            else
                return TournamentStatus.Completed;
        }

        private string GetTournamentStatusString(TournamentDto tournament)
        {
            return GetTournamentStatus(tournament) switch
            {
                TournamentStatus.Upcoming => "Предстоящий",
                TournamentStatus.Active => "Активный",
                TournamentStatus.Completed => "Завершенный",
                _ => "Неизвестно"
            };
        }

        private void ApplyFilters()
        {
            var filtred = _allTournaments.Where(t =>
            {
                var textMatch = string.IsNullOrEmpty(SearchText) ||
                                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                (t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                                (t.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

                var statusMatch = SelectedStatusFilter == "Все" || 
                                GetTournamentStatusString(t) == SelectedStatusFilter;

                return textMatch && statusMatch;
            }).ToList();

            Tournaments.Clear();
            foreach (var tournament in filtred)
            {
                Tournaments.Add(tournament);
            }
        }

        public string GetStatusForTournament(TournamentDto tournament)
        {
            return GetTournamentStatusString(tournament);
        }
    }
}
