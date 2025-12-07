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

        // Перемещение первого участника вправо (в следующий раунд)
        [RelayCommand]
        private void MoveRight(MatchItemViewModel match)
        {
            if (!IsTournamentEditable || match == null || SelectedBracketIndex < 0) return;
            // Разрешить перенос первого участника только если второй ещё не переносился
            if (match.SecondMoved)
            {
                MessageBox.Show("Из одного поединка можно перенести только одного участника.", "Внимание");
                return;
            }
            var category = Brackets[SelectedBracketIndex];
            var currentRoundIndex = match.Round;
            if (currentRoundIndex >= category.Rounds.Count) return; // последний раунд

            var nextRoundIndex = currentRoundIndex + 1;
            if (nextRoundIndex > category.Rounds.Count) return;

            var nextRound = category.Rounds[nextRoundIndex - 1];
            // Определяем индекс боя в следующем раунде: ближайший соответствующий (по порядку пар)
            var targetMatchIndex = Math.Max(1, (match.Order + 1) / 2); // ceil(Order/2)
            if (targetMatchIndex > nextRound.Items.Count) targetMatchIndex = nextRound.Items.Count;
            var target = nextRound.Items[targetMatchIndex - 1];
            // Заполняем слот первого участника
            if (string.IsNullOrEmpty(target.FirstParticipantName) || target.FirstParticipantTournamentCategoryId == 0)
            {
                target.FirstParticipantTournamentCategoryId = match.FirstParticipantTournamentCategoryId;
                target.FirstParticipantName = match.FirstParticipantName;
            }
            else if (!target.SecondParticipantTournamentCategoryId.HasValue || string.IsNullOrEmpty(target.SecondParticipantName))
            {
                target.SecondParticipantTournamentCategoryId = match.FirstParticipantTournamentCategoryId;
                target.SecondParticipantName = match.FirstParticipantName;
            }
            else
            {
                // если оба слота заняты, ищем ближайший следующий свободный
                var fallback = nextRound.Items.FirstOrDefault(i => i.FirstParticipantTournamentCategoryId == 0 || string.IsNullOrEmpty(i.FirstParticipantName) || !i.SecondParticipantTournamentCategoryId.HasValue || string.IsNullOrEmpty(i.SecondParticipantName));
                if (fallback == null) return;
                if (fallback.FirstParticipantTournamentCategoryId == 0 || string.IsNullOrEmpty(fallback.FirstParticipantName))
                {
                    fallback.FirstParticipantTournamentCategoryId = match.FirstParticipantTournamentCategoryId;
                    fallback.FirstParticipantName = match.FirstParticipantName;
                }
                else
                {
                    fallback.SecondParticipantTournamentCategoryId = match.FirstParticipantTournamentCategoryId;
                    fallback.SecondParticipantName = match.FirstParticipantName;
                }
            }
            // Оставляем имя в исходной позиции
            match.FirstMoved = true;
        }

        // Перемещение первого участника влево (в предыдущий раунд на исходную позицию)
        [RelayCommand]
        private void MoveLeft(MatchItemViewModel match)
        {
            if (!IsTournamentEditable || match == null || SelectedBracketIndex < 0) return;
            var category = Brackets[SelectedBracketIndex];
            var currentRoundIndex = match.Round;
            if (currentRoundIndex <= 1) return; // первый раунд

            var prevRoundIndex = currentRoundIndex - 1;
            var prevRound = category.Rounds[prevRoundIndex - 1];
            // найти исходную позицию по Id
            var origin = prevRound.Items.FirstOrDefault(i => i.FirstParticipantTournamentCategoryId == match.FirstParticipantTournamentCategoryId || i.SecondParticipantTournamentCategoryId == match.FirstParticipantTournamentCategoryId);
            if (origin != null)
            {
                if (origin.FirstParticipantTournamentCategoryId == match.FirstParticipantTournamentCategoryId)
                {
                    origin.FirstParticipantName = match.FirstParticipantName;
                }
                else if (origin.SecondParticipantTournamentCategoryId == match.FirstParticipantTournamentCategoryId)
                {
                    origin.SecondParticipantName = match.FirstParticipantName;
                }
                // удалить из текущего раунда
                if (match != null)
                {
                    // очистить из слота, если он был скопирован
                    var currentRound = category.Rounds[currentRoundIndex - 1];
                    foreach (var itm in currentRound.Items)
                    {
                        if (itm.FirstParticipantTournamentCategoryId == match.FirstParticipantTournamentCategoryId)
                        {
                            itm.FirstParticipantTournamentCategoryId = 0;
                            itm.FirstParticipantName = string.Empty;
                            match.FirstMoved = false;
                            break;
                        }
                        if (itm.SecondParticipantTournamentCategoryId == match.FirstParticipantTournamentCategoryId)
                        {
                            itm.SecondParticipantTournamentCategoryId = null;
                            itm.SecondParticipantName = null;
                            match.FirstMoved = false;
                            break;
                        }
                    }
                }
            }
        }

        // Аналогичные команды для второго участника
        [RelayCommand]
        private void MoveRightSecond(MatchItemViewModel match)
        {
            if (!IsTournamentEditable || match == null || SelectedBracketIndex < 0 || !match.SecondParticipantTournamentCategoryId.HasValue) return;
            // Разрешить перенос второго участника только если первый ещё не переносился
            if (match.FirstMoved)
            {
                MessageBox.Show("Из одного поединка можно перенести только одного участника.", "Внимание");
                return;
            }
            var category = Brackets[SelectedBracketIndex];
            var currentRoundIndex = match.Round;
            if (currentRoundIndex >= category.Rounds.Count) return;
            var nextRound = category.Rounds[currentRoundIndex];
            // Индекс боя в следующем раунде аналогично: ceil(Order/2)
            var targetMatchIndex = Math.Max(1, (match.Order + 1) / 2);
            if (targetMatchIndex > nextRound.Items.Count) targetMatchIndex = nextRound.Items.Count;
            var target = nextRound.Items[targetMatchIndex - 1];
            // Заполняем слот второго участника
            if (!target.SecondParticipantTournamentCategoryId.HasValue || string.IsNullOrEmpty(target.SecondParticipantName))
            {
                target.SecondParticipantTournamentCategoryId = match.SecondParticipantTournamentCategoryId;
                target.SecondParticipantName = match.SecondParticipantName;
            }
            else if (target.FirstParticipantTournamentCategoryId == 0 || string.IsNullOrEmpty(target.FirstParticipantName))
            {
                target.FirstParticipantTournamentCategoryId = match.SecondParticipantTournamentCategoryId.Value;
                target.FirstParticipantName = match.SecondParticipantName;
            }
            else
            {
                var fallback = nextRound.Items.FirstOrDefault(i => !i.SecondParticipantTournamentCategoryId.HasValue || string.IsNullOrEmpty(i.SecondParticipantName) || i.FirstParticipantTournamentCategoryId == 0 || string.IsNullOrEmpty(i.FirstParticipantName));
                if (fallback == null) return;
                if (!fallback.SecondParticipantTournamentCategoryId.HasValue || string.IsNullOrEmpty(fallback.SecondParticipantName))
                {
                    fallback.SecondParticipantTournamentCategoryId = match.SecondParticipantTournamentCategoryId;
                    fallback.SecondParticipantName = match.SecondParticipantName;
                }
                else
                {
                    fallback.FirstParticipantTournamentCategoryId = match.SecondParticipantTournamentCategoryId.Value;
                    fallback.FirstParticipantName = match.SecondParticipantName;
                }
            }
            match.SecondMoved = true;
        }

        [RelayCommand]
        private void MoveLeftSecond(MatchItemViewModel match)
        {
            if (!IsTournamentEditable || match == null || SelectedBracketIndex < 0 || !match.SecondParticipantTournamentCategoryId.HasValue) return;
            var category = Brackets[SelectedBracketIndex];
            var currentRoundIndex = match.Round;
            if (currentRoundIndex <= 1) return;
            var prevRound = category.Rounds[currentRoundIndex - 2];
            var origin = prevRound.Items.FirstOrDefault(i => i.FirstParticipantTournamentCategoryId == match.SecondParticipantTournamentCategoryId || i.SecondParticipantTournamentCategoryId == match.SecondParticipantTournamentCategoryId);
            if (origin != null)
            {
                if (origin.FirstParticipantTournamentCategoryId == match.SecondParticipantTournamentCategoryId)
                    origin.FirstParticipantName = match.SecondParticipantName;
                else if (origin.SecondParticipantTournamentCategoryId == match.SecondParticipantTournamentCategoryId)
                    origin.SecondParticipantName = match.SecondParticipantName;

                // очистить текущий слот
                var currentRound = category.Rounds[currentRoundIndex - 1];
                foreach (var itm in currentRound.Items)
                {
                    if (itm.FirstParticipantTournamentCategoryId == match.SecondParticipantTournamentCategoryId)
                    {
                        itm.FirstParticipantTournamentCategoryId = 0;
                        itm.FirstParticipantName = string.Empty;
                        match.SecondMoved = false;
                        break;
                    }
                    if (itm.SecondParticipantTournamentCategoryId == match.SecondParticipantTournamentCategoryId)
                    {
                        itm.SecondParticipantTournamentCategoryId = null;
                        itm.SecondParticipantName = null;
                        match.SecondMoved = false;
                        break;
                    }
                }
            }
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
            // Группируем матчи по раундам
            var actualByRound = Matches
                .GroupBy(x => x.Round <= 0 ? 1 : x.Round)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Order <= 0 ? int.MaxValue : x.Order).ToList());

            // Формируем список участников из первого раунда
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
            participants = participants.Distinct().Where(p => p.id != 0).ToList();

            // Вычисляем размер сетки и число раундов (фиксированное количество поединков)
            static int NextPow2(int x){ if (x<=1) return 1; x--; x|=x>>1; x|=x>>2; x|=x>>4; x|=x>>8; x|=x>>16; return x+1; }
            static int Log2(int x){ int r=0; while((x>>=1)>0) r++; return r; }
            int bracketSize = NextPow2(participants.Count);
            int roundsCount = Math.Max(1, Log2(bracketSize));

            Rounds.Clear();

            // Первый раунд: заполняем участниками (используем фактические матчи, иначе создаём плейсхолдеры)
            var round1Items = new ObservableCollection<MatchItemViewModel>();
            int ord1 = 1;
            if (r1 != null && r1.Count > 0)
            {
                foreach (var m in r1)
                {
                    round1Items.Add(new MatchItemViewModel
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
                        Round = 1,
                        Order = ord1++
                    });
                }
            }
            else
            {
                // создаём плейсхолдеры пар из участников; при нечётном числе возможен bye (второй участник null)
                for (int i = 0; i < bracketSize; i += 2)
                {
                    var a = i < participants.Count ? participants[i] : (0, string.Empty);
                    var b = (i + 1) < participants.Count ? participants[i + 1] : (0, string.Empty);
                    round1Items.Add(new MatchItemViewModel
                    {
                        MatchId = 0,
                        FirstParticipantTournamentCategoryId = a.Item1,
                        SecondParticipantTournamentCategoryId = b.Item1 == 0 ? null : b.Item1,
                        FirstParticipantName = a.Item2,
                        SecondParticipantName = b.Item1 == 0 ? null : b.Item2,
                        FirstParticipantScore = 0,
                        SecondParticipantScore = 0,
                        IsStarted = false,
                        IsFinished = false,
                        Round = 1,
                        Order = ord1++
                    });
                }
            }
            Rounds.Add(new BracketRoundViewModel("Раунд 1", round1Items));

            // Остальные раунды: фиксированное количество матчей-плейсхолдеров, без участников
            int matchesInRound = bracketSize / 2;
            for (int r = 2; r <= roundsCount; r++)
            {
                matchesInRound /= 2;
                var items = new ObservableCollection<MatchItemViewModel>();
                for (int i = 1; i <= Math.Max(1, matchesInRound); i++)
                {
                    items.Add(new MatchItemViewModel
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
                        Order = i
                    });
                }
                Rounds.Add(new BracketRoundViewModel($"Раунд {r}", items));
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
        [ObservableProperty] private bool firstMoved;
        [ObservableProperty] private bool secondMoved;

        public bool CanEdit => IsStarted && !IsFinished && MatchId != 0;
        partial void OnIsStartedChanged(bool value) => OnPropertyChanged(nameof(CanEdit));
        partial void OnIsFinishedChanged(bool value) => OnPropertyChanged(nameof(CanEdit));
    }
}
