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
        private readonly MainViewModel _mainViewModel;

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

        public BracketsViewModel(ApiService apiService, TournamentDto tournament, bool isOrganizer, MainViewModel mainViewModel)
        {
            _apiService = apiService;
            _tournament = tournament;
            _mainViewModel = mainViewModel;
            IsOrganizer = isOrganizer;

            // Determine editability from current time within tournament dates (Active)
            var now = DateTime.Now;
            IsTournamentEditable = now >= _tournament.StartDate && now <= _tournament.EndDate;

            SelectedBracketIndex = 0;
            LoadBracketsCommand.Execute(null);
        }

        [RelayCommand]
        private void Back()
        {
            _mainViewModel.NavigateToTournamentDetails(_tournament);
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

        // Команды начать/завершить и сетевые вызовы сохранены из логики. Все изменения выполняются локально, серверная фиксация только при закрытии категории.

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

            // Local helpers
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

            // Main loop until champion
            var current = new List<(int id, string name)>(participants);
            var builtRounds = new List<BracketRoundViewModel>();
            ObservableCollection<MatchItemViewModel>? semifinalRound = null;

            int roundIndex = 1;
            while (current.Count > 0)
            {
                var roundItems = new ObservableCollection<MatchItemViewModel>();

                // Prefer overlay of actual matches for this round
                if (actualByRound.TryGetValue(roundIndex, out var actual) && actual.Count > 0)
                {
                    int ord = 1;
                    foreach (var m in actual)
                    {
                        var cm = new MatchItemViewModel
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
                            Round = roundIndex,
                            Order = ord++
                        };
                        roundItems.Add(cm);
                    }
                }
                else
                {
                    // Synthesize matches from current participants
                    int ord = 1;
                    int count = current.Count;
                    bool hasBye = (count % 2) == 1;
                    int pairs = count / 2; // integer division
                    int idx = 0;
                    for (int p = 0; p < pairs; p++)
                    {
                        var a = current[idx++];
                        var b = current[idx++];
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
                    }
                    if (hasBye)
                    {
                        var bye = current.Last();
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
                    }
                }

                var roundVm = new BracketRoundViewModel($"Раунд {roundIndex}", roundItems);
                builtRounds.Add(roundVm);

                // Track semifinal round (the round before final): when next round will have 2 participants and current had exactly 2 real matches (no byes)
                // Semifinal condition: roundItems contains exactly 2 items and both have two participants
                if (roundItems.Count == 2 && roundItems.All(i => i.SecondParticipantTournamentCategoryId.HasValue))
                {
                    semifinalRound = roundItems;
                }

                // Compute next participants from winners
                var next = new List<(int id, string name)>();
                foreach (var m in roundItems)
                {
                    // bye member goes through automatically
                    if (m.SecondParticipantTournamentCategoryId == null)
                    {
                        next.Add((m.FirstParticipantTournamentCategoryId, m.FirstParticipantName));
                        continue;
                    }

                    // Use decided winner; equal scores are not allowed by rule, but in case of placeholder, stop progression
                    if (m.MatchId == 0 && m.FirstParticipantScore == m.SecondParticipantScore)
                    {
                        next.Clear();
                        break;
                    }

                    var w = DecideWinnerLocal(m);
                    next.Add(w);
                }

                if (next.Count == 0)
                    break;
                if (next.Count == 1)
                {
                    // Final already decided; champion found
                    current = next;
                    break;
                }
                current = next;
                roundIndex++;
            }

            // Commit built rounds to Rounds
            foreach (var r in builtRounds)
                Rounds.Add(r);

            // Bronze match: only if semifinal existed with two real matches (no byes)
            if (semifinalRound != null)
            {
                var losers = new List<(int id, string name)>();
                foreach (var m in semifinalRound)
                {
                    if (!m.SecondParticipantTournamentCategoryId.HasValue)
                    {
                        losers.Clear();
                        break; // bye present: no bronze match
                    }
                    losers.Add(DecideLoserLocal(m));
                }

                if (losers.Count == 2)
                {
                    var bronzeItems = new ObservableCollection<MatchItemViewModel>();
                    bronzeItems.Add(new MatchItemViewModel
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
                    });
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
