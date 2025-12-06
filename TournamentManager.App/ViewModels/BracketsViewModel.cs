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

            (int id, string name) DecideWinnerLocal(MatchItemViewModel m)
            {
                if (m.SecondParticipantTournamentCategoryId == null)
                    return (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName);
                return (m.FirstParticipantScore > m.SecondParticipantScore)
                    ? (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName)
                    : (m.SecondParticipantTournamentCategoryId!.Value, m.SecondParticipantName ?? string.Empty);
            }
            (int id, string name) DecideLoserLocal(MatchItemViewModel m)
            {
                if (m.SecondParticipantTournamentCategoryId == null)
                    return (0, string.Empty);
                return (m.FirstParticipantScore < m.SecondParticipantScore)
                    ? (m.FirstParticipantTournamentCategoryId, m.FirstParticipantName)
                    : (m.SecondParticipantTournamentCategoryId!.Value, m.SecondParticipantName ?? string.Empty);
            }

            var actualByRound = Matches
                .GroupBy(x => x.Round <= 0 ? 1 : x.Round)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order <= 0 ? int.MaxValue : x.Order).ToList());

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

            var current = new List<(int id, string name)>(participants);
            var builtRounds = new List<BracketRoundViewModel>();
            ObservableCollection<MatchItemViewModel>? semifinalRound = null;

            int roundIndex = 1;
            while (current.Count > 0)
            {
                var roundItems = new ObservableCollection<MatchItemViewModel>();
                var usedInRound = new HashSet<int>(); // enforce uniqueness per round

                if (actualByRound.TryGetValue(roundIndex, out var actual) && actual.Count > 0)
                {
                    int ord = 1;
                    foreach (var m in actual)
                    {
                        int a = m.FirstParticipantTournamentCategoryId;
                        int b = m.SecondParticipantTournamentCategoryId ?? 0;
                        if (usedInRound.Contains(a) || (b != 0 && usedInRound.Contains(b)))
                            continue; // skip duplicates to enforce single match per participant per round

                        var cm = new MatchItemViewModel
                        {
                            MatchId = m.MatchId,
                            FirstParticipantTournamentCategoryId = a,
                            SecondParticipantTournamentCategoryId = m.SecondParticipantTournamentCategoryId,
                            FirstParticipantName = m.FirstParticipantName,
                            SecondParticipantName = m.SecondParticipantName,
                            FirstParticipantScore = m.FirstParticipantScore,
                            SecondParticipantScore = m.SecondParticipantScore,
                            IsStarted = m.IsStarted,
                            IsFinished = m.IsFinished,
                            Round = roundIndex,
                            Order = ord++
                        };
                        roundItems.Add(cm);
                        usedInRound.Add(a);
                        if (b != 0) usedInRound.Add(b);
                    }
                }
                else
                {
                    int ord = 1;
                    int count = current.Count;
                    bool hasBye = (count % 2) == 1;
                    int pairs = count / 2;
                    int idx = 0;
                    for (int p = 0; p < pairs; p++)
                    {
                        var a = current[idx++];
                        var b = current[idx++];
                        if (usedInRound.Contains(a.id) || usedInRound.Contains(b.id))
                            continue;
                        roundItems.Add(new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = a.id,
                            SecondParticipantTournamentCategoryId = b.id,
                            FirstParticipantName = a.name,
                            SecondParticipantName = b.name,
                            FirstParticipantScore = 0,
                            SecondParticipantScore = 0,
                            IsStarted = false,
                            IsFinished = false,
                            Round = roundIndex,
                            Order = ord++
                        });
                        usedInRound.Add(a.id);
                        usedInRound.Add(b.id);
                    }
                    if (hasBye)
                    {
                        var bye = current.Last();
                        if (!usedInRound.Contains(bye.id))
                        {
                            roundItems.Add(new MatchItemViewModel
                            {
                                MatchId = 0,
                                FirstParticipantTournamentCategoryId = bye.id,
                                SecondParticipantTournamentCategoryId = null,
                                FirstParticipantName = bye.name,
                                SecondParticipantName = null,
                                FirstParticipantScore = 0,
                                SecondParticipantScore = 0,
                                IsStarted = false,
                                IsFinished = false,
                                Round = roundIndex,
                                Order = pairs + 1
                            });
                            usedInRound.Add(bye.id);
                        }
                    }
                }

                var roundVm = new BracketRoundViewModel($"Раунд {roundIndex}", roundItems);
                builtRounds.Add(roundVm);

                if (roundItems.Count == 2 && roundItems.All(i => i.SecondParticipantTournamentCategoryId.HasValue))
                {
                    semifinalRound = roundItems;
                }

                var next = new List<(int id, string name)>();
                var usedNext = new HashSet<int>();
                foreach (var m in roundItems)
                {
                    if (m.SecondParticipantTournamentCategoryId == null)
                    {
                        if (!usedNext.Contains(m.FirstParticipantTournamentCategoryId))
                        {
                            next.Add((m.FirstParticipantTournamentCategoryId, m.FirstParticipantName));
                            usedNext.Add(m.FirstParticipantTournamentCategoryId);
                        }
                        continue;
                    }

                    if (m.MatchId == 0 && m.FirstParticipantScore == m.SecondParticipantScore)
                    {
                        next.Clear();
                        break;
                    }

                    var w = DecideWinnerLocal(m);
                    if (!usedNext.Contains(w.id))
                    {
                        next.Add(w);
                        usedNext.Add(w.id);
                    }
                }

                if (next.Count == 0)
                    break;
                if (next.Count == 1)
                {
                    current = next;
                    break;
                }
                current = next;
                roundIndex++;
            }

            foreach (var r in builtRounds)
                Rounds.Add(r);

            if (semifinalRound != null)
            {
                var losers = new List<(int id, string name)>();
                foreach (var m in semifinalRound)
                {
                    if (!m.SecondParticipantTournamentCategoryId.HasValue)
                    {
                        losers.Clear();
                        break;
                    }
                    var l = DecideLoserLocal(m);
                    if (!losers.Any(x => x.id == l.id) && l.id != 0)
                        losers.Add(l);
                }

                if (losers.Count == 2)
                {
                    var bronzeItems = new ObservableCollection<MatchItemViewModel>
                    {
                        new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = losers[0].id,
                            SecondParticipantTournamentCategoryId = losers[1].id,
                            FirstParticipantName = losers[0].name,
                            SecondParticipantName = losers[1].name,
                            FirstParticipantScore = 0,
                            SecondParticipantScore = 0,
                            IsStarted = false,
                            IsFinished = false,
                            Round = builtRounds.Count + 1,
                            Order = 1
                        }
                    };
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
