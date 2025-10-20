using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class TournamentsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IService<TournamentDto> _tournamentService;

        [ObservableProperty]
        private ObservableCollection<TournamentDto> _tournaments = new();

        [ObservableProperty]
        private TournamentDto? _selectedTournament;

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

        public TournamentsViewModel(ApiService apiService, IService<TournamentDto> tournamentService)
        {
            _apiService = apiService;
            _tournamentService = tournamentService;

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
                    Tournaments.Clear();
                    foreach (var tournament in tournaments)
                    {
                        if (tournament is not null)
                            Tournaments.Add(tournament);
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
        private void CreateTournament()
        {
            MessageBox.Show("Переход к созданию турнира", "Создание турнира");
        }

        [RelayCommand]
        private void EditTournament()
        {
            if (SelectedTournament is null)
            {
                MessageBox.Show("Выберите турнир для редактирования", "Внимание");

                return;
            }

            MessageBox.Show($"Редактирование турнира: {SelectedTournament.Name}", "Редактирование");
        }

        [RelayCommand]
        private async Task DeleteTournament()
        {
            if (SelectedTournament is null)
            {
                MessageBox.Show("Выберите турнир для удаления", "Внимание");

                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить турнир \"{SelectedTournament.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
                );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _tournamentService.DeleteAsync(SelectedTournament.Id);

                    MessageBox.Show("Турнир успешно удален", "Успех");
                    await LoadTournaments();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления турнира: {ex.Message}", "Ошибка");
                }
            }
        }

        [RelayCommand]
        private void ViewTournamentDetails()
        {
            if (SelectedTournament is null)
            {
                MessageBox.Show("Выберите турнир для просмотра", "Внимание");

                return;
            }

            MessageBox.Show($"Детали турнира: {SelectedTournament.Name}", "Просмотр");
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedStatusFilterChanged(string value)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtred = Tournaments.Where(t =>
                SelectedStatusFilter == "Все" || t.Status == SelectedStatusFilter
            ).Where(t =>
                string.IsNullOrEmpty(SearchText) ||
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            Tournaments.Clear();
            foreach (var tournament in filtred)
            {
                Tournaments.Add(tournament);
            }
        }

        public ObservableCollection<TournamentDto> FiltredTournaments => new(
            Tournaments.Where(t =>
                (SelectedStatusFilter == "Все" || t.Status == SelectedStatusFilter) &&
                (string.IsNullOrEmpty(SearchText) || t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            );
    }
}
