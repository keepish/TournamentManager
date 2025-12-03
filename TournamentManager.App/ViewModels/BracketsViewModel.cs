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

        [ObservableProperty]
        private bool isTournamentEditable;

        public BracketsViewModel(ApiService apiService, TournamentDto tournament, bool isOrganizer)
        {
            _apiService = apiService;
            _tournament = tournament;
            IsOrganizer = isOrganizer;

            // Determine editability from current time within tournament dates (Active)
            var now = DateTime.Now;
            IsTournamentEditable = now >= _tournament.StartDate && now <= _tournament.EndDate;

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
            if (!IsTournamentEditable) return;
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
            if (!IsTournamentEditable) return;
            if (match == null || match.IsFinished || match.MatchId == 0 || SelectedBracketIndex < 0) return;
            var category = Brackets[SelectedBracketIndex];
            if (category.IsCategoryFinished) return;
            match.IsStarted = true;
        }

        [RelayCommand]
        private async Task FinishMatch(MatchItemViewModel match)
        {
            if (!IsTournamentEditable) return;
            if (match == null || !match.IsStarted || match.IsFinished || match.MatchId == 0 || SelectedBracketIndex < 0) return;
            var category = Brackets[SelectedBracketIndex];
            if (category.IsCategoryFinished) return;

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
            if (!IsTournamentEditable) return;
            if (!IsOrganizer || match == null || match.MatchId == 0 || SelectedBracketIndex < 0) return;
            var category = Brackets[SelectedBracketIndex];
            if (category.IsCategoryFinished) return;
            if (!match.IsFinished) return;

            match.IsFinished = false;
            match.IsStarted = true;
        }

        [RelayCommand]
        private void FinishCategory()
        {
            if (!IsTournamentEditable) return;
            if (SelectedBracketIndex < 0) return;
            var category = Brackets[SelectedBracketIndex];
            category.ComputePodium();
            category.IsCategoryFinished = true;
        }
    }

    public partial class CategoryBracketItemViewModel : ObservableObject
    {
        [ObservableProperty] private int tournamentCategoryId;
        [ObservableProperty] private int categoryId;
        [ObservableProperty] private string categoryDisplay = string.Empty;
        [ObservableProperty] private ObservableCollection<MatchItemViewModel> matches = new();

        public ObservableCollection<BracketRoundViewModel> Rounds { get; } = new();

        [ObservableProperty] private bool isCategoryFinished;
        [ObservableProperty] private string podiumGold;
        [ObservableProperty] private string podiumSilver;
        [ObservableProperty] private string podiumBronze;

        public void ComputePodium()
        {
            // Find final: last round with a pair
            var final = Rounds.LastOrDefault(r => r.Items.Any(i => i.SecondParticipantTournamentCategoryId.HasValue));
            if (final != null)
            {
                var lastMatch = final.Items.LastOrDefault(i => i.SecondParticipantTournamentCategoryId.HasValue);
                if (lastMatch != null)
                {
                    if (lastMatch.FirstParticipantScore > lastMatch.SecondParticipantScore)
                    {
                        PodiumGold = lastMatch.FirstParticipantName;
                        PodiumSilver = lastMatch.SecondParticipantName ?? string.Empty;
                    }
                    else
                    {
                        PodiumGold = lastMatch.SecondParticipantName ?? string.Empty;
                        PodiumSilver = lastMatch.FirstParticipantName;
                    }
                }
            }
            // Find bronze round added as "Матч за 3-е место"
            var bronzeRound = Rounds.LastOrDefault(r => r.Title.Contains("3-е место"));
            if (bronzeRound != null)
            {
                var bm = bronzeRound.Items.FirstOrDefault();
                if (bm != null)
                {
                    PodiumBronze = (bm.FirstParticipantScore > bm.SecondParticipantScore)
                        ? bm.FirstParticipantName
                        : (bm.SecondParticipantName ?? string.Empty);
                }
            }
        }

        public void BuildRoundsStructured()
        {
            Rounds.Clear();
            if (Matches.Count == 0)
            {
                OnPropertyChanged(nameof(Rounds));
                return;
            }

            // Group actual matches by round in order
            var actualByRound = Matches
                .GroupBy(x => x.Round <= 0 ? 1 : x.Round)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order <= 0 ? int.MaxValue : x.Order).ToList());

            MatchItemViewModel Clone(MatchItemViewModel src) => new()
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
                Round = src.Round <= 0 ? 1 : src.Round,
                Order = src.Order
            };

            // Helper: decide winner of a match based on scores
            (int id, string name) DecideWinner(MatchItemViewModel m)
            {
                if (m.SecondParticipantTournamentCategoryId == null)
                    return (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName);
                if (m.FirstParticipantScore > m.SecondParticipantScore)
                    return (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName);
                if (m.SecondParticipantTournamentCategoryId.HasValue && m.SecondParticipantScore >= m.FirstParticipantScore)
                    return (m.SecondParticipantTournamentCategoryId.Value, m.SecondParticipantName ?? string.Empty);
                return (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName);
            }
            (int id, string name) DecideLoser(MatchItemViewModel m)
            {
                if (m.SecondParticipantTournamentCategoryId == null)
                    return (0, string.Empty); // bye has no loser
                if (m.FirstParticipantScore < m.SecondParticipantScore)
                    return (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName);
                if (m.SecondParticipantTournamentCategoryId.HasValue && m.SecondParticipantScore <= m.FirstParticipantScore)
                    return (m.SecondParticipantTournamentCategoryId.Value, m.SecondParticipantName ?? string.Empty);
                return (m.SecondParticipantTournamentCategoryId ?? 0, m.SecondParticipantName ?? string.Empty);
            }

            // Build initial participants list from round 1 actual matches; fall back to seeds from all matches
            var participants = new List<(int id, string name)>();
            if (actualByRound.TryGetValue(1, out var r1))
            {
                foreach (var m in r1)
                {
                    participants.Add((m.FirstParticipantTournamentCategoryId, m.FirstParticipantName));
                    if (m.SecondParticipantTournamentCategoryId.HasValue)
                        participants.Add((m.SecondParticipantTournamentCategoryId.Value, m.SecondParticipantName ?? string.Empty));
                }
            }
            if (participants.Count == 0)
            {
                foreach (var m in Matches)
                {
                    participants.Add((m.FirstParticipantTournamentCategoryId, m.FirstParticipantName));
                    if (m.SecondParticipantTournamentCategoryId.HasValue)
                        participants.Add((m.SecondParticipantTournamentCategoryId.Value, m.SecondParticipantName ?? string.Empty));
                }
                participants = participants.Distinct().OrderBy(x => x.id).ToList();
            }

            // Build rounds iteratively until champion
            var roundIndex = 1;
            var currentParticipants = new List<(int id, string name)>(participants);
            ObservableCollection<MatchItemViewModel>? semifinalRoundItems = null;

            while (currentParticipants.Count > 0)
            {
                var roundItems = new ObservableCollection<MatchItemViewModel>();

                // use actual matches for this round if available
                if (actualByRound.TryGetValue(roundIndex, out var actualRoundMatches) && actualRoundMatches.Count > 0)
                {
                    int ord = 1;
                    foreach (var m in actualRoundMatches)
                    {
                        var cm = Clone(m);
                        cm.Round = roundIndex;
                        cm.Order = ord++;
                        roundItems.Add(cm);
                    }
                }
                else
                {
                    // Synthesize matches from current participants following universal algorithm
                    int ord = 1;
                    int count = currentParticipants.Count;
                    bool hasBye = (count % 2) == 1;
                    int pairs = hasBye ? (count - 1) / 2 : count / 2;

                    // Pair sequentially
                    int idx = 0;
                    for (int p = 0; p < pairs; p++)
                    {
                        var a = currentParticipants[idx++];
                        var b = currentParticipants[idx++];
                        roundItems.Add(new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = a.id,
                            SecondParticipantTournamentCategoryId = b.id,
                            FirstParticipantName = a.name,
                            SecondParticipantName = b.name,
                            Round = roundIndex,
                            Order = ord++,
                            IsStarted = false,
                            IsFinished = false,
                            FirstParticipantScore = 0,
                            SecondParticipantScore = 0
                        });
                    }

                    // Bye participant (if odd count)
                    if (hasBye)
                    {
                        var bye = currentParticipants.Last();
                        roundItems.Add(new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = bye.id,
                            SecondParticipantTournamentCategoryId = null,
                            FirstParticipantName = bye.name,
                            SecondParticipantName = null,
                            Round = roundIndex,
                            Order = ord++,
                            IsStarted = false,
                            IsFinished = false
                        });
                    }
                }

                // Append this round to UI
                Rounds.Add(new BracketRoundViewModel($"Раунд {roundIndex}", roundItems));

                // If next round leads to final, remember semifinal matches to compute losers later
                if (currentParticipants.Count == 4 || (currentParticipants.Count == 3 && roundItems.Any(i => i.SecondParticipantTournamentCategoryId == null)))
                {
                    semifinalRoundItems = roundItems;
                }

                // Determine participants for next round
                var nextParticipants = new List<(int id, string name)>();
                foreach (var m in roundItems)
                {
                    // bye passes automatically
                    if (m.SecondParticipantTournamentCategoryId == null)
                    {
                        nextParticipants.Add((m.FirstParticipantTournamentCategoryId, m.FirstParticipantName));
                        continue;
                    }

                    // If we had actual data, use scores; otherwise keep placeholders (no scores) and stop progression
                    if (m.MatchId != 0 || m.IsFinished || m.FirstParticipantScore != m.SecondParticipantScore)
                    {
                        var w = DecideWinner(m);
                        nextParticipants.Add(w);
                    }
                    else
                    {
                        // stop building further rounds until results are available
                        nextParticipants.Clear();
                        break;
                    }
                }

                // Champion condition
                if (nextParticipants.Count == 0)
                    break;
                if (nextParticipants.Count == 1)
                {
                    // final champion round - build last match only if previous round contained a pair leading to single
                    currentParticipants = nextParticipants;
                    if (roundItems.Count == 1 && roundItems[0].SecondParticipantTournamentCategoryId == null)
                    {
                        // nothing more to build
                        break;
                    }
                    // will exit next loop iteration after adding final
                }
                else
                {
                    currentParticipants = nextParticipants;
                }

                roundIndex++;
            }

            // Append bronze match at the very end after finals
            if (semifinalRoundItems != null)
            {
                var losers = new List<(int id, string name)>();
                foreach (var m in semifinalRoundItems)
                {
                    if (m.SecondParticipantTournamentCategoryId == null) continue; // bye has no loser
                    var l = DecideLoser(m);
                    if (l.id != 0) losers.Add(l);
                }

                if (losers.Count == 2)
                {
                    var bronzeItems = new ObservableCollection<MatchItemViewModel>();
                    // Try to overlay actual bronze match (use highest existing round + 1)
                    int bronzeRoundIndex = (Rounds.Any() ? Rounds.Count + 1 : 1);
                    var maxRoundKey = actualByRound.Keys.DefaultIfEmpty(0).Max();
                    if (actualByRound.TryGetValue(bronzeRoundIndex, out var bronzeActual) && bronzeActual.Count > 0)
                    {
                        int ord = 1;
                        foreach (var m in bronzeActual)
                        {
                            var cm = Clone(m);
                            cm.Round = bronzeRoundIndex; cm.Order = ord++;
                            bronzeItems.Add(cm);
                        }
                    }
                    else
                    {
                        bronzeItems.Add(new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = losers[0].id,
                            SecondParticipantTournamentCategoryId = losers[1].id,
                            FirstParticipantName = losers[0].name,
                            SecondParticipantName = losers[1].name,
                            Round = bronzeRoundIndex,
                            Order = 1,
                            IsStarted = false,
                            IsFinished = false
                        });
                    }

                    Rounds.Add(new BracketRoundViewModel("Матч за 3-е место", bronzeItems));
                }
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
