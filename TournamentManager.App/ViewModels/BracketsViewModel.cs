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

        public void BuildRoundsStructured()
        {
            Rounds.Clear();
            if (Matches.Count == 0)
            {
                OnPropertyChanged(nameof(Rounds));
                return;
            }

            // Collect participant ids in seed order
            var participantIds = new HashSet<int>();
            foreach (var m in Matches)
            {
                participantIds.Add(m.FirstParticipantTournamentCategoryId);
                if (m.SecondParticipantTournamentCategoryId.HasValue)
                    participantIds.Add(m.SecondParticipantTournamentCategoryId.Value);
            }
            var seedOrdered = participantIds.OrderBy(id => id).ToList();
            int participantsCount = seedOrdered.Count;

            // Name map
            var nameById = new Dictionary<int, string>();
            foreach (var m in Matches)
            {
                if (!string.IsNullOrWhiteSpace(m.FirstParticipantName))
                    nameById[m.FirstParticipantTournamentCategoryId] = m.FirstParticipantName;
                if (m.SecondParticipantTournamentCategoryId.HasValue && !string.IsNullOrWhiteSpace(m.SecondParticipantName))
                    nameById[m.SecondParticipantTournamentCategoryId.Value] = m.SecondParticipantName!;
            }

            // Group actual matches by round
            var actualByRound = Matches
                .GroupBy(x => x.Round <= 0 ? 1 : x.Round)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order).ToList());

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

            // ROUND 1
            var round1Items = new ObservableCollection<MatchItemViewModel>();
            var usedInRound = new HashSet<int>();
            var actualR1 = actualByRound.ContainsKey(1) ? actualByRound[1] : new List<MatchItemViewModel>();

            // Add actual matches ensuring uniqueness
            int orderCounter = 1;
            foreach (var m in actualR1)
            {
                int a = m.FirstParticipantTournamentCategoryId;
                int b = m.SecondParticipantTournamentCategoryId ?? 0;
                if (usedInRound.Contains(a) || (b != 0 && usedInRound.Contains(b)))
                    continue; // skip duplicate participant match

                round1Items.Add(Clone(m));
                usedInRound.Add(a);
                if (b != 0) usedInRound.Add(b);
            }

            // Placeholder matches for remaining participants (pairing sequentially)
            var remaining = seedOrdered.Where(id => !usedInRound.Contains(id)).ToList();
            for (int i = 0; i < remaining.Count; i += 2)
            {
                int first = remaining[i];
                int? second = (i + 1 < remaining.Count) ? remaining[i + 1] : null;
                var item = new MatchItemViewModel
                {
                    MatchId = 0,
                    FirstParticipantTournamentCategoryId = first,
                    SecondParticipantTournamentCategoryId = second,
                    FirstParticipantName = nameById.TryGetValue(first, out var fn) ? fn : string.Empty,
                    SecondParticipantName = second.HasValue && nameById.TryGetValue(second.Value, out var sn) ? sn : null,
                    Round = 1,
                    Order = orderCounter++,
                    IsStarted = false,
                    IsFinished = false
                };
                round1Items.Add(item);
            }

            // Normalize order indices
            int idx = 1; foreach (var r1 in round1Items) r1.Order = idx++;
            Rounds.Add(new BracketRoundViewModel("Раунд 1", round1Items));

            // Subsequent rounds - derive from winners of previous round; ensure uniqueness
            var prevRound = round1Items;
            int roundNumber = 2;
            while (prevRound.Count > 1)
            {
                var current = new ObservableCollection<MatchItemViewModel>();
                var winnerCandidates = new List<(int id, string name)>();

                foreach (var match in prevRound)
                {
                    // Decide winner if finished and scores differ OR if only one participant present
                    if (match.SecondParticipantTournamentCategoryId == null)
                    {
                        winnerCandidates.Add((match.FirstParticipantTournamentCategoryId, match.FirstParticipantName));
                        continue;
                    }
                    if (match.IsFinished && match.FirstParticipantScore != match.SecondParticipantScore)
                    {
                        if (match.FirstParticipantScore > match.SecondParticipantScore)
                            winnerCandidates.Add((match.FirstParticipantTournamentCategoryId, match.FirstParticipantName));
                        else
                            winnerCandidates.Add((match.SecondParticipantTournamentCategoryId.Value, match.SecondParticipantName ?? string.Empty));
                    }
                    else
                    {
                        // Undecided placeholder
                        winnerCandidates.Add((0, string.Empty));
                    }
                }

                // Build actual matches of this round if they exist, enforcing uniqueness
                var actualCurrent = actualByRound.ContainsKey(roundNumber) ? actualByRound[roundNumber] : new List<MatchItemViewModel>();
                var used = new HashSet<int>();
                int order = 1;
                foreach (var m in actualCurrent)
                {
                    int a = m.FirstParticipantTournamentCategoryId;
                    int b = m.SecondParticipantTournamentCategoryId ?? 0;
                    if ((a != 0 && used.Contains(a)) || (b != 0 && used.Contains(b)))
                        continue;
                    var clone = Clone(m);
                    clone.Round = roundNumber;
                    clone.Order = order++;
                    current.Add(clone);
                    if (a != 0) used.Add(a);
                    if (b != 0) used.Add(b);
                }

                // Add placeholders for winners without a match yet
                var pending = winnerCandidates.Select(w => w.id).Where(id => id != 0).Where(id => !used.Contains(id)).ToList();
                for (int i = 0; i < pending.Count; i += 2)
                {
                    int first = pending[i];
                    int? second = (i + 1 < pending.Count) ? pending[i + 1] : null;
                    current.Add(new MatchItemViewModel
                    {
                        MatchId = 0,
                        FirstParticipantTournamentCategoryId = first,
                        SecondParticipantTournamentCategoryId = second,
                        FirstParticipantName = nameById.TryGetValue(first, out var fName) ? fName : string.Empty,
                        SecondParticipantName = second.HasValue && nameById.TryGetValue(second.Value, out var sName) ? sName : null,
                        Round = roundNumber,
                        Order = order++,
                        IsStarted = false,
                        IsFinished = false
                    });
                }

                if (current.Count == 0) break; // no progression possible
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
