using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;
using TournamentManager.Core.DTOs.Matches;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

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
        private bool isTournamentEditable = true;

        public BracketsViewModel(ApiService apiService, TournamentDto tournament, bool isOrganizer)
        {
            _apiService = apiService;
            _tournament = tournament;
            IsOrganizer = isOrganizer;
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
                    {
                        if (b == null) continue;
                        var cb = new CategoryBracketItemViewModel
                        {
                            TournamentCategoryId = b.TournamentCategoryId,
                            CategoryId = b.CategoryId,
                            CategoryDisplay = b.CategoryDisplay
                        };

                        // round 1: actual matches
                        foreach (var m in b.Matches)
                        {
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
                                IsFinished = m.IsFinished
                            });
                        }

                        cb.Rounds.Clear();
                        // Round 1: all matches
                        cb.Rounds.Add(new RoundViewModel
                        {
                            Title = "Раунд 1",
                            Items = cb.Matches,
                            IsFirstRound = true,
                            IsLastRound = false
                        });

                        // Round 2: empty placeholders (half size)
                        int round2Count = System.Math.Max(1, cb.Matches.Count / 2);
                        var round2 = new RoundViewModel
                        {
                            Title = "Раунд 2",
                            Items = new ObservableCollection<MatchItemViewModel>(),
                            IsFirstRound = false,
                            IsLastRound = false
                        };
                        for (int i = 0; i < round2Count; i++)
                        {
                            round2.Items.Add(new MatchItemViewModel
                            {
                                MatchId = 0,
                                FirstParticipantTournamentCategoryId = 0,
                                SecondParticipantTournamentCategoryId = null,
                                FirstParticipantName = string.Empty,
                                SecondParticipantName = string.Empty,
                                FirstParticipantScore = 0,
                                SecondParticipantScore = 0,
                                IsStarted = false,
                                IsFinished = false
                            });
                        }
                        cb.Rounds.Add(round2);

                        // Round 3: final (1 empty placeholder)
                        var round3 = new RoundViewModel
                        {
                            Title = "Финал",
                            Items = new ObservableCollection<MatchItemViewModel>(),
                            IsFirstRound = false,
                            IsLastRound = true
                        };
                        round3.Items.Add(new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = 0,
                            SecondParticipantTournamentCategoryId = null,
                            FirstParticipantName = string.Empty,
                            SecondParticipantName = string.Empty,
                            FirstParticipantScore = 0,
                            SecondParticipantScore = 0,
                            IsStarted = false,
                            IsFinished = false
                        });
                        cb.Rounds.Add(round3);

                        Brackets.Add(cb);
                    }
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

        // Persist all matches of the selected category and advance brackets server-side
        [RelayCommand]
        private async Task FinishCategory()
        {
            if (SelectedBracketIndex < 0 || SelectedBracketIndex >= Brackets.Count)
                return;

            var bracket = Brackets[SelectedBracketIndex];

            try
            {
                IsLoading = true;

                // Save scores for all matches in this bracket
                foreach (var match in bracket.Matches)
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

                    // Tell backend to advance winners for finished matches
                    if (match.IsFinished)
                    {
                        await _apiService.PostAsync<object>($"api/Matches/{match.MatchId}/advance", new { });
                    }
                }

                bracket.IsCategoryFinished = true;
                IsTournamentEditable = false;

                await LoadBrackets();
                MessageBox.Show("Категория зафиксирована. Результаты сохранены.", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фиксации категории: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void StartMatch(MatchItemViewModel match)
        {
            if (match == null || match.IsFinished) return;
            match.IsStarted = true;
        }

        [RelayCommand]
        private void FinishMatch(MatchItemViewModel match)
        {
            if (match == null || !match.IsStarted || match.IsFinished) return;

            // mark finished locally; do not persist until FinishCategory
            match.IsFinished = true;
        }

        [RelayCommand]
        private void EditMatch(MatchItemViewModel match)
        {
            if (!IsOrganizer || match == null) return;
            if (!match.IsFinished) return;

            match.IsFinished = false;
            match.IsStarted = true;
        }

        // Move participant from a source match/slot to a target match/slot locally (client-side only)
        public Task<bool> MoveParticipantAsync(MatchItemViewModel sourceMatch, int sourceSlot, MatchItemViewModel targetMatch, int targetSlot)
        {
            try
            {
                // Only one participant can be moved from a pair, and only once
                if (sourceMatch.PairMoveLocked)
                    return Task.FromResult(false);

                // Read source
                int sourceId = sourceSlot == 1 ? sourceMatch.FirstParticipantTournamentCategoryId : (sourceMatch.SecondParticipantTournamentCategoryId ?? 0);
                string sourceName = sourceSlot == 1 ? sourceMatch.FirstParticipantName : (sourceMatch.SecondParticipantName ?? string.Empty);
                if (sourceId == 0) return Task.FromResult(false);

                // Place in target
                if (targetSlot == 1)
                {
                    if (targetMatch.FirstParticipantTournamentCategoryId != 0) return Task.FromResult(false);
                    targetMatch.FirstParticipantTournamentCategoryId = sourceId;
                    targetMatch.FirstParticipantName = sourceName;
                    targetMatch.FirstParticipantLocked = true;
                }
                else
                {
                    if (targetMatch.SecondParticipantTournamentCategoryId.HasValue && targetMatch.SecondParticipantTournamentCategoryId.Value != 0)
                        return Task.FromResult(false);
                    targetMatch.SecondParticipantTournamentCategoryId = sourceId;
                    targetMatch.SecondParticipantName = sourceName;
                    targetMatch.SecondParticipantLocked = true;
                }

                // Lock the source pair to forbid any further moves from this match (only one move allowed)
                sourceMatch.FirstParticipantLocked = true;
                sourceMatch.SecondParticipantLocked = true;
                sourceMatch.PairMoveLocked = true;

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        // Move participant by rounds: direction -1 (left/previous round), +1 (right/next round)
        public Task<bool> MoveParticipantByRounds(MatchItemViewModel match, int slot, int direction)
        {
            if (match == null || match.PairMoveLocked) return Task.FromResult(false);
            if (SelectedBracketIndex < 0 || SelectedBracketIndex >= Brackets.Count) return Task.FromResult(false);
            var bracket = Brackets[SelectedBracketIndex];
            if (bracket.Rounds == null || bracket.Rounds.Count == 0) return Task.FromResult(false);

            // Find current round and index
            int currentRoundIdx = -1;
            int currentIndexInRound = -1;
            for (int i = 0; i < bracket.Rounds.Count; i++)
            {
                var round = bracket.Rounds[i];
                currentIndexInRound = round.Items.IndexOf(match);
                if (currentIndexInRound >= 0) { currentRoundIdx = i; break; }
            }
            if (currentRoundIdx == -1) return Task.FromResult(false);

            int targetRoundIdx = currentRoundIdx + direction;
            if (targetRoundIdx < 0 || targetRoundIdx >= bracket.Rounds.Count) return Task.FromResult(false);
            var targetRound = bracket.Rounds[targetRoundIdx];

            // Compute preferred index mapping
            int preferredIndex = currentIndexInRound;
            if (direction > 0)
                preferredIndex = System.Math.Max(0, currentIndexInRound / 2);
            else if (direction < 0)
                preferredIndex = System.Math.Min(targetRound.Items.Count - 1, currentIndexInRound * 2);

            int sourceId = slot == 1 ? match.FirstParticipantTournamentCategoryId : (match.SecondParticipantTournamentCategoryId ?? 0);
            string sourceName = slot == 1 ? match.FirstParticipantName : (match.SecondParticipantName ?? string.Empty);
            if (sourceId == 0) return Task.FromResult(false);

            // Prefer nearest slot in existing matches: check if preferred match has an empty opposite slot first
            int chosenIdx = -1;
            int chosenSlot = slot; // default to requested slot, but may fill opposite if it's empty

            bool HasEmpty(int idx, int s)
            {
                if (idx < 0 || idx >= targetRound.Items.Count) return false;
                var m = targetRound.Items[idx];
                return s == 1 ? m.FirstParticipantTournamentCategoryId == 0 : (!m.SecondParticipantTournamentCategoryId.HasValue || m.SecondParticipantTournamentCategoryId.Value == 0);
            }

            // 1) Try preferred index, opposite slot first (to fill existing pair)
            int opposite = slot == 1 ? 2 : 1;
            if (HasEmpty(preferredIndex, opposite))
            {
                chosenIdx = preferredIndex;
                chosenSlot = opposite;
            }
            else if (HasEmpty(preferredIndex, slot))
            {
                chosenIdx = preferredIndex;
                chosenSlot = slot;
            }
            else
            {
                // 2) Search nearest by distance; always prefer opposite slot when available to avoid creating new matches unnecessarily
                int bestIdx = -1;
                int bestSlot = slot;
                int bestDist = int.MaxValue;
                for (int i = 0; i < targetRound.Items.Count; i++)
                {
                    int dist = System.Math.Abs(i - preferredIndex);
                    var m = targetRound.Items[i];
                    bool emptyOpp = opposite == 1 ? m.FirstParticipantTournamentCategoryId == 0 : (!m.SecondParticipantTournamentCategoryId.HasValue || m.SecondParticipantTournamentCategoryId.Value == 0);
                    bool emptySame = slot == 1 ? m.FirstParticipantTournamentCategoryId == 0 : (!m.SecondParticipantTournamentCategoryId.HasValue || m.SecondParticipantTournamentCategoryId.Value == 0);

                    // Prefer opposite slot empties to complete pairs
                    if (emptyOpp && dist < bestDist)
                    {
                        bestDist = dist;
                        bestIdx = i;
                        bestSlot = opposite;
                    }
                    else if (emptySame && dist < bestDist)
                    {
                        bestDist = dist;
                        bestIdx = i;
                        bestSlot = slot;
                    }
                }
                chosenIdx = bestIdx;
                chosenSlot = bestSlot;
            }

            if (chosenIdx == -1) return Task.FromResult(false);
            var target = targetRound.Items[chosenIdx];

            // Place
            if (chosenSlot == 1)
            {
                target.FirstParticipantTournamentCategoryId = sourceId;
                target.FirstParticipantName = sourceName;
                target.FirstParticipantLocked = true;
            }
            else
            {
                target.SecondParticipantTournamentCategoryId = sourceId;
                target.SecondParticipantName = sourceName;
                target.SecondParticipantLocked = true;
            }

            // Lock source pair to prevent further moves
            match.FirstParticipantLocked = true;
            match.SecondParticipantLocked = true;
            match.PairMoveLocked = true;

            // Mark category dirty
            var category = Brackets.FirstOrDefault(b => b.Rounds.Contains(targetRound));
            if (category != null) category.IsDirty = true;

            OnPropertyChanged(nameof(Brackets));
            return Task.FromResult(true);
        }
    }

    public partial class CategoryBracketItemViewModel : ObservableObject
    {
        [ObservableProperty] private int tournamentCategoryId;
        [ObservableProperty] private int categoryId;
        [ObservableProperty] private string categoryDisplay = string.Empty;
        [ObservableProperty] private ObservableCollection<MatchItemViewModel> matches = new();
        [ObservableProperty] private ObservableCollection<RoundViewModel> rounds = new();
        [ObservableProperty] private bool isCategoryFinished;
        [ObservableProperty] private string podiumGold = string.Empty;
        [ObservableProperty] private string podiumSilver = string.Empty;
        [ObservableProperty] private string podiumBronze = string.Empty;
        [ObservableProperty] private bool isDirty;

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
                if (!m.IsFinished)
                    return (0, string.Empty);
                return (m.FirstParticipantScore > m.SecondParticipantScore)
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
                    participants.Add((id: m.FirstParticipantTournamentCategoryId, name: m.FirstParticipantName));
                    if (m.SecondParticipantTournamentCategoryId.HasValue)
                        participants.Add((id: m.SecondParticipantTournamentCategoryId.Value, name: m.SecondParticipantName ?? string.Empty));
                }
            }
            if (participants.Count == 0)
            {
                foreach (var m in Matches)
                {
                    participants.Add((id: m.FirstParticipantTournamentCategoryId, name: m.FirstParticipantName));
                    if (m.SecondParticipantTournamentCategoryId.HasValue)
                        participants.Add((id: m.SecondParticipantTournamentCategoryId.Value, name: m.SecondParticipantName ?? string.Empty));
                }
                participants = participants.Distinct().OrderBy(x => x.id).ToList();
            }

            // Determine round match counts based on actual pairs:
            // round1 = initial pairs count; each next round = ceil(prev/2), until 1
            int initialPairs = participants.Count / 2; // if odd, last becomes a bye and pairs = floor(count/2)
            if (actualByRound.TryGetValue(1, out var realRound1))
            {
                initialPairs = realRound1.Count;
            }
            if (initialPairs <= 0)
            {
                // fallback from participants list (pair up sequentially)
                initialPairs = Math.Max(1, participants.Count / 2);
            }

            var matchCounts = new List<int>();
            int cur = initialPairs;
            while (cur > 0)
            {
                matchCounts.Add(cur);
                if (cur == 1) break;
                cur = (cur + 1) / 2; // ceil
            }

            var builtRounds = new List<ObservableCollection<MatchItemViewModel>>();

            for (int roundIndex = 0; roundIndex < matchCounts.Count; roundIndex++)
            {
                int r = roundIndex + 1;
                int matchesCount = matchCounts[roundIndex];
                var roundItems = new ObservableCollection<MatchItemViewModel>();

                // create placeholders
                for (int i = 0; i < matchesCount; i++)
                {
                    roundItems.Add(new MatchItemViewModel
                    {
                        MatchId = 0,
                        FirstParticipantTournamentCategoryId = 0,
                        SecondParticipantTournamentCategoryId = null,
                        FirstParticipantName = string.Empty,
                        SecondParticipantName = null,
                        FirstParticipantScore = 0,
                        SecondParticipantScore = 0,
                        IsStarted = false,
                        IsFinished = false,
                        Round = r,
                        Order = i + 1,
                        FirstParticipantLocked = false,
                        SecondParticipantLocked = false
                    });
                }

                // populate with real matches if any
                if (actualByRound.TryGetValue(r, out var realMatches) && realMatches.Any())
                {
                    foreach (var real in realMatches)
                    {
                        int pos = (real.Order > 0 && real.Order <= matchesCount) ? (real.Order - 1) : -1;
                        if (pos == -1)
                        {
                            // next free slot
                            for (int i = 0; i < matchesCount; i++)
                            {
                                if (roundItems[i].MatchId == 0) { pos = i; break; }
                            }
                        }
                        if (pos >= 0 && pos < matchesCount)
                        {
                            roundItems[pos] = new MatchItemViewModel
                            {
                                MatchId = real.MatchId,
                                FirstParticipantTournamentCategoryId = real.FirstParticipantTournamentCategoryId,
                                SecondParticipantTournamentCategoryId = real.SecondParticipantTournamentCategoryId,
                                FirstParticipantName = real.FirstParticipantName,
                                SecondParticipantName = real.SecondParticipantName,
                                FirstParticipantScore = real.FirstParticipantScore,
                                SecondParticipantScore = real.SecondParticipantScore,
                                IsStarted = real.IsStarted,
                                IsFinished = real.IsFinished,
                                Round = r,
                                Order = pos + 1,
                                FirstParticipantLocked = real.FirstParticipantTournamentCategoryId != 0,
                                SecondParticipantLocked = real.SecondParticipantTournamentCategoryId.HasValue && real.SecondParticipantTournamentCategoryId.Value != 0
                            };
                        }
                    }
                }
                else if (r == 1)
                {
                    // populate round1 from participants sequentially
                    for (int i = 0; i < matchesCount; i++)
                    {
                        int idxA = i * 2;
                        int idxB = idxA + 1;
                        var a = idxA < participants.Count ? participants[idxA] : (id: 0, name: string.Empty);
                        var b = idxB < participants.Count ? participants[idxB] : (id: 0, name: string.Empty);
                        roundItems[i] = new MatchItemViewModel
                        {
                            MatchId = 0,
                            FirstParticipantTournamentCategoryId = a.id,
                            SecondParticipantTournamentCategoryId = b.id == 0 ? null : b.id,
                            FirstParticipantName = a.name,
                            SecondParticipantName = string.IsNullOrWhiteSpace(b.name) ? null : b.name,
                            FirstParticipantScore = 0,
                            SecondParticipantScore = 0,
                            IsStarted = false,
                            IsFinished = false,
                            Round = 1,
                            Order = i + 1,
                            FirstParticipantLocked = a.id != 0,
                            SecondParticipantLocked = b.id != 0
                        };
                    }
                }

                builtRounds.Add(roundItems);
            }

            // materialize into Rounds
            for (int i = 0; i < builtRounds.Count; i++)
            {
                var title = i == 0 ? "Раунд 1" : (i == builtRounds.Count - 1 ? $"Раунд {i + 1} (Финал)" : $"Раунд {i + 1}");
                var rr = new RoundViewModel
                {
                    Title = title,
                    Items = builtRounds[i],
                    IsFirstRound = (i == 0),
                    IsLastRound = (i == builtRounds.Count - 1)
                };
                Rounds.Add(rr);
            }

            OnPropertyChanged(nameof(Rounds));
        }
    }

    public partial class RoundViewModel : ObservableObject
    {
        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private ObservableCollection<MatchItemViewModel> items = new();
        [ObservableProperty] private bool isFirstRound;
        [ObservableProperty] private bool isLastRound;
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

        // Lock flags used by drag-drop logic to prevent changes once placed
        [ObservableProperty] private bool firstParticipantLocked;
        [ObservableProperty] private bool secondParticipantLocked;
        // Prevent moving more than one participant from the pair
        [ObservableProperty] private bool pairMoveLocked;
        [ObservableProperty] private int round;
        [ObservableProperty] private int order;

        public bool CanEdit => IsStarted && !IsFinished;

        partial void OnIsStartedChanged(bool value) => OnPropertyChanged(nameof(CanEdit));
        partial void OnIsFinishedChanged(bool value) => OnPropertyChanged(nameof(CanEdit));
    }
}
