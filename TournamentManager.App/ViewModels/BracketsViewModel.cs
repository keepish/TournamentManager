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
        private bool isOrganizer;

        [ObservableProperty]
        private ObservableCollection<CategoryBracketItemViewModel> brackets = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int selectedBracketIndex;

        public BracketsViewModel(ApiService apiService, TournamentDto tournament, bool isOrganizer)
        {
            _apiService = apiService;
            _tournament = tournament;
            IsOrganizer = isOrganizer;
            SelectedBracketIndex = 0;
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

                var oldIndex = SelectedBracketIndex;
                Brackets.Clear();
                if (data == null)
                    return;

                foreach (var b in data)
                {
                    if (b == null) continue;
                    var cb = new CategoryBracketItemViewModel
                    {
                        TournamentCategoryId = b.TournamentCategoryId,
                        CategoryId = b.CategoryId,
                        CategoryDisplay = b.CategoryDisplay
                    };

                    if (b.Matches != null)
                    {
                        foreach (var m in b.Matches)
                        {
                            if (m == null) continue;
                            cb.Matches.Add(new MatchItemViewModel
                            {
                                MatchId = m.MatchId,
                                FirstParticipantTournamentCategoryId = m.FirstParticipantTournamentCategoryId,
                                SecondParticipantTournamentCategoryId = m.SecondParticipantTournamentCategoryId,
                                FirstParticipantName = m.FirstParticipantName,
                                SecondParticipantName = m.SecondParticipantName,
                                FirstParticipantScore = m.FirstParticipantScore,
                                SecondParticipantScore = m.SecondParticipantScore,
                                IsStarted = m.IsStarted,
                                IsFinished = m.IsFinished,
                                Round = m.Round,
                                Order = m.Order
                            });
                        }
                    }

                    cb.BuildRoundsStructured();
                    Brackets.Add(cb);
                }

                // restore selection
                if (oldIndex >= 0 && oldIndex < Brackets.Count)
                {
                    SelectedBracketIndex = oldIndex;
                }
                else
                {
                    SelectedBracketIndex = Brackets.Count > 0 ? 0 : -1;
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
        private async Task SaveMatchScore(MatchItemViewModel match)
        {
            if (match == null || match.MatchId == 0)
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

                await _apiService.PutAsync<object>($"api/Matches/{dto.Id}", dto);
                MessageBox.Show("Результат сохранен", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить результат: {ex.Message}", "Ошибка");
            }
        }

        [RelayCommand]
        private void StartMatch(MatchItemViewModel match)
        {
            if (match == null || match.IsFinished || match.MatchId == 0) return;
            match.IsStarted = true;
        }

        [RelayCommand]
        private async Task FinishMatch(MatchItemViewModel match)
        {
            if (match == null || !match.IsStarted || match.IsFinished || match.MatchId == 0) return;

            match.IsFinished = true;

            await SaveMatchScore(match);

            try
            {
                await _apiService.PostAsync<object>($"api/Matches/{match.MatchId}/advance", new { });
                await LoadBrackets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось продвинуть победителя: {ex.Message}", "Ошибка");
            }
        }

        [RelayCommand]
        private void EditMatch(MatchItemViewModel match)
        {
            if (!IsOrganizer || match == null || match.MatchId == 0) return;
            if (!match.IsFinished) return;

            match.IsFinished = false;
            match.IsStarted = true;
        }
    }

    public partial class CategoryBracketItemViewModel : ObservableObject
    {
        [ObservableProperty] private int tournamentCategoryId;
        [ObservableProperty] private int categoryId;
        [ObservableProperty] private string categoryDisplay = string.Empty;
        [ObservableProperty] private ObservableCollection<MatchItemViewModel> matches = new();

        public ObservableCollection<BracketRoundViewModel> Rounds { get; } = new();

        private static int NextPow2(int x)
        {
            if (x <= 1) return 1;
            x--;
            x |= x >> 1; x |= x >> 2; x |= x >> 4; x |= x >> 8; x |= x >> 16;
            return x + 1;
        }
        private static int Log2(int x)
        {
            int r = 0; while ((x >>= 1) > 0) r++; return r;
        }

        public void BuildRoundsStructured()
        {
            Rounds.Clear();
            if (Matches.Count == 0)
            {
                OnPropertyChanged(nameof(Rounds));
                return;
            }

            // Seed order by participant tournament category id
            var ptcIds = new HashSet<int>();
            foreach (var m in Matches)
            {
                ptcIds.Add(m.FirstParticipantTournamentCategoryId);
                if (m.SecondParticipantTournamentCategoryId.HasValue) ptcIds.Add(m.SecondParticipantTournamentCategoryId.Value);
            }
            var seed = ptcIds.OrderBy(id => id).ToList();
            int participants = seed.Count;

            // Map ptcId -> display name (from any match)
            var nameByPtc = new Dictionary<int, string>();
            foreach (var m in Matches)
            {
                if (!string.IsNullOrWhiteSpace(m.FirstParticipantName)) nameByPtc[m.FirstParticipantTournamentCategoryId] = m.FirstParticipantName;
                if (m.SecondParticipantTournamentCategoryId.HasValue && !string.IsNullOrWhiteSpace(m.SecondParticipantName)) nameByPtc[m.SecondParticipantTournamentCategoryId.Value] = m.SecondParticipantName!;
            }

            // Actual matches by (round, order)
            var actualByKey = Matches
                .GroupBy(x => x.Round <= 0 ? 1 : x.Round)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order).ToList());

            // Helper to clone a view-model item
            MatchItemViewModel CloneFrom(MatchItemViewModel src)
            {
                return new MatchItemViewModel
                {
                    MatchId = src.MatchId,
                    FirstParticipantTournamentCategoryId = src.FirstParticipantTournamentCategoryId,
                    SecondParticipantTournamentCategoryId = src.SecondParticipantTournamentCategoryId,
                    FirstParticipantName = src.FirstParticipantName,
                    SecondParticipantName = src.SecondParticipantName,
                    FirstParticipantScore = src.FirstParticipantScore,
                    SecondParticipantScore = src.SecondParticipantScore,
                    IsStarted = src.IsStarted,
                    IsFinished = src.IsFinished,
                    Round = src.Round,
                    Order = src.Order
                };
            }

            // Round 1: expected matches = ceil(participants/2) to avoid extra empty pair
            var round1 = new ObservableCollection<MatchItemViewModel>();
            int expectedR1 = Math.Max(1, (participants + 1) / 2);
            var actualR1 = actualByKey.ContainsKey(1) ? actualByKey[1] : new List<MatchItemViewModel>();
            for (int i = 0; i < expectedR1; i++)
            {
                var real = actualR1.FirstOrDefault(x => (x.Order > 0 ? x.Order - 1 : i) == i) ?? actualR1.ElementAtOrDefault(i);
                if (real != null)
                {
                    round1.Add(CloneFrom(real));
                }
                else
                {
                    int aIndex = 2 * i;
                    int bIndex = 2 * i + 1;
                    // Only create placeholder if there is at least one seeded participant
                    bool hasA = aIndex < seed.Count;
                    bool hasB = bIndex < seed.Count;
                    if (!hasA && !hasB) continue;

                    var item = new MatchItemViewModel
                    {
                        MatchId = 0,
                        FirstParticipantTournamentCategoryId = hasA ? seed[aIndex] : 0,
                        SecondParticipantTournamentCategoryId = hasB ? seed[bIndex] : null,
                        FirstParticipantName = (hasA && nameByPtc.TryGetValue(seed[aIndex], out var aName)) ? aName : string.Empty,
                        SecondParticipantName = (hasB && nameByPtc.TryGetValue(seed[bIndex], out var bName)) ? bName : null,
                        IsStarted = false,
                        IsFinished = false,
                        Round = 1,
                        Order = i + 1
                    };
                    round1.Add(item);
                }
            }
            Rounds.Add(new BracketRoundViewModel($"Раунд 1", round1));

            // Subsequent rounds: derive from previous round winners; expected = ceil(prevRound.Count/2)
            var prevRound = round1;
            int roundNumber = 2;
            while (prevRound.Count > 1)
            {
                var current = new ObservableCollection<MatchItemViewModel>();
                int expected = Math.Max(1, (prevRound.Count + 1) / 2);
                var actual = actualByKey.ContainsKey(roundNumber) ? actualByKey[roundNumber] : new List<MatchItemViewModel>();

                for (int i = 0; i < expected; i++)
                {
                    var left = prevRound.ElementAtOrDefault(2 * i);
                    var right = prevRound.ElementAtOrDefault(2 * i + 1);

                    string? leftWinner = null;
                    int leftPtc = 0;
                    if (left != null)
                    {
                        bool leftDecided = (left.SecondParticipantName == null) ||
                                           ((left.FirstParticipantScore + left.SecondParticipantScore) > 0 && left.FirstParticipantScore != left.SecondParticipantScore);
                        if (leftDecided)
                        {
                            if (left.SecondParticipantName == null || left.FirstParticipantScore > left.SecondParticipantScore)
                            {
                                leftWinner = string.IsNullOrWhiteSpace(left.FirstParticipantName) ? null : left.FirstParticipantName;
                                leftPtc = left.FirstParticipantTournamentCategoryId;
                            }
                            else
                            {
                                leftWinner = string.IsNullOrWhiteSpace(left.SecondParticipantName) ? null : left.SecondParticipantName;
                                leftPtc = left.SecondParticipantTournamentCategoryId ?? 0;
                            }
                        }
                    }

                    string? rightWinner = null;
                    int rightPtc = 0;
                    if (right != null)
                    {
                        bool rightDecided = (right.SecondParticipantName == null) ||
                                            ((right.FirstParticipantScore + right.SecondParticipantScore) > 0 && right.FirstParticipantScore != right.SecondParticipantScore);
                        if (rightDecided)
                        {
                            if (right.SecondParticipantName == null || right.FirstParticipantScore > right.SecondParticipantScore)
                            {
                                rightWinner = string.IsNullOrWhiteSpace(right.FirstParticipantName) ? null : right.FirstParticipantName;
                                rightPtc = right.FirstParticipantTournamentCategoryId;
                            }
                            else
                            {
                                rightWinner = string.IsNullOrWhiteSpace(right.SecondParticipantName) ? null : right.SecondParticipantName;
                                rightPtc = right.SecondParticipantTournamentCategoryId ?? 0;
                            }
                        }
                    }

                    var placeholder = new MatchItemViewModel
                    {
                        MatchId = 0,
                        FirstParticipantTournamentCategoryId = leftPtc,
                        SecondParticipantTournamentCategoryId = rightPtc == 0 ? null : rightPtc,
                        FirstParticipantName = leftWinner ?? string.Empty,
                        SecondParticipantName = rightWinner,
                        IsStarted = false,
                        IsFinished = false,
                        Round = roundNumber,
                        Order = i + 1
                    };

                    var real = actual.FirstOrDefault(x => (x.Order > 0 ? x.Order - 1 : i) == i) ?? actual.ElementAtOrDefault(i);
                    if (real != null)
                    {
                        placeholder.MatchId = real.MatchId;
                        placeholder.FirstParticipantTournamentCategoryId = real.FirstParticipantTournamentCategoryId;
                        placeholder.SecondParticipantTournamentCategoryId = real.SecondParticipantTournamentCategoryId;
                        placeholder.FirstParticipantName = string.IsNullOrWhiteSpace(real.FirstParticipantName) ? placeholder.FirstParticipantName : real.FirstParticipantName;
                        placeholder.SecondParticipantName = string.IsNullOrWhiteSpace(real.SecondParticipantName) ? placeholder.SecondParticipantName : real.SecondParticipantName;
                        placeholder.FirstParticipantScore = real.FirstParticipantScore;
                        placeholder.SecondParticipantScore = real.SecondParticipantScore;
                        placeholder.IsStarted = real.IsStarted;
                        placeholder.IsFinished = real.IsFinished;
                    }

                    current.Add(placeholder);
                }

                Rounds.Add(new BracketRoundViewModel($"Раунд {roundNumber}", current));
                prevRound = current;
                roundNumber++;
            }

            OnPropertyChanged(nameof(Rounds));
        }
    }

    public class BracketRoundViewModel
    {
        public string Title { get; }
        public ObservableCollection<MatchItemViewModel> Items { get; }
        public BracketRoundViewModel(string title, ObservableCollection<MatchItemViewModel> items)
        {
            Title = title;
            Items = items;
        }
    }

    public partial class MatchItemViewModel : ObservableObject
    {
        [ObservableProperty] private int matchId;
        [ObservableProperty] private int firstParticipantTournamentCategoryId;
        [ObservableProperty] private int? secondParticipantTournamentCategoryId;
        [ObservableProperty] private string firstParticipantName = string.Empty;
        [ObservableProperty] private string? secondParticipantName;
        [ObservableProperty] private int firstParticipantScore;
        [ObservableProperty] private int secondParticipantScore;
        [ObservableProperty] private bool isStarted;
        [ObservableProperty] private bool isFinished;
        [ObservableProperty] private int round;
        [ObservableProperty] private int order;

        public bool CanEdit => IsStarted && !IsFinished && MatchId != 0;

        partial void OnIsStartedChanged(bool value) => OnPropertyChanged(nameof(CanEdit));
        partial void OnIsFinishedChanged(bool value) => OnPropertyChanged(nameof(CanEdit));
    }
}
