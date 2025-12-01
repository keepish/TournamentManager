using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;
using TournamentManager.Core.DTOs.Matches;

namespace TournamentManager.Client.ViewModels
{
    public partial class BracketsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly TournamentDto _tournament;

        [ObservableProperty]
        private ObservableCollection<CategoryBracketDto> brackets = new();

        [ObservableProperty]
        private bool isLoading;

        public BracketsViewModel(ApiService apiService, TournamentDto tournament)
        {
            _apiService = apiService;
            _tournament = tournament;
            LoadBracketsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadBrackets()
        {
            IsLoading = true;
            try
            {
                var endpoint = $"api/Tournaments/{_tournament.Id}/brackets";
                var data = await _apiService.GetAsync<List<CategoryBracketDto>>(endpoint);
                Brackets.Clear();
                if (data != null)
                {
                    foreach (var b in data)
                        if (b != null)
                            Brackets.Add(b);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сетки: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveMatchScore(BracketMatchItemDto match)
        {
            if (match == null)
                return;

            try
            {
                var dto = new MatchDto
                {
                    Id = match.MatchId,
                    FirstParticipantId = match.FirstParticipantTournamentCategoryId,
                    SecondParticipantId = match.SecondParticipantTournamentCategoryId,
                    FirstParticipantScore = match.FirstParticipantScore,
                    SecondParticipantScore = match.SecondParticipantScore
                };

                // MatchesController PUT returns 204 NoContent; ignore body deserialization
                await _apiService.PutAsync<object>($"api/Matches/{dto.Id}", dto);
                MessageBox.Show("Результат сохранен", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить результат: {ex.Message}", "Ошибка");
            }
        }
    }
}
