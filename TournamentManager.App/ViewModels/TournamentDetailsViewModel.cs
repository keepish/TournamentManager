using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Participants;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class TournamentDetailsViewModel : ObservableObject
    {
        private readonly TournamentDto _tournament;
        private readonly ApiService _apiService;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private ObservableCollection<ParticipantDto> participants = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        public string TournamentName => _tournament?.Name ?? "Детали турнира";
        public string TournamentDates => $"{_tournament?.StartDate:dd.MM.yyyy} - {_tournament?.EndDate:dd.MM.yyyy}";
        public string TournamentAddress => _tournament?.Address ?? "";
        public string TournamentDescription => _tournament?.Description ?? "";

        public TournamentDetailsViewModel(TournamentDto tournament, ApiService apiService, MainViewModel mainViewModel) 
        {
            _tournament = tournament;
            _apiService = apiService;
            _mainViewModel = mainViewModel;

            LoadParticipants();
        }

        [RelayCommand]
        private async Task LoadParticipants()
        {
            IsLoading = true;

            try
            {
                await Task.Delay(1000);

                Participants.Clear();

                Participants.Add(new ParticipantDto
                {
                    Id = 1,
                    Name = "Иван",
                    Surname = "Петров",
                    Patronymic = "Сергеевич",
                    Phone = "+7 (999) 123-45-67",
                    Gender = 1,
                    Birthday = new DateTime(1995, 5, 15),
                    Weight = 70.5m
                });

                Participants.Add(new ParticipantDto
                {
                    Id = 2,
                    Name = "Мария",
                    Surname = "Сидорова",
                    Patronymic = "Александровна",
                    Phone = "+7 (999) 765-43-21",
                    Gender = 2,
                    Birthday = new DateTime(1998, 8, 22),
                    Weight = 58.2m
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки участников: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RegisterParticipants()
        {
            if (!HasUnsavedChanges)
            {
                MessageBox.Show("Нет изменений для сохранения", "Информация");
                return;
            }
            try
            {
                IsLoading = true;

                // TODO: Реализовать сохранение участников в БД
                var validationErrors = ValidateParticipants();
                if (validationErrors.Any())
                {
                    MessageBox.Show($"Ошибки валидации:\n{string.Join("\n", validationErrors)}", "Ошибка");
                    return;
                }

                await Task.Delay(1000); // Имитация сохранения

                HasUnsavedChanges = false;
                MessageBox.Show("Участники успешно зарегистрированы!", "Успех");

                // Обновляем список чтобы получить актуальные ID из БД
                await LoadParticipants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации участников: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false; 
            }
        }

        private List<string> ValidateParticipants()
        {
            var errors = new List<string>();

            for (int i = 0; i < Participants.Count; i++)
            {
                var participant = Participants[i];

                if (string.IsNullOrWhiteSpace(participant.Name))
                    errors.Add($"Строка {i + 1}: Имя обязательно для заполнения");

                if (string.IsNullOrWhiteSpace(participant.Surname))
                    errors.Add($"Строка {i + 1}: Фамилия обязательна для заполнения");

                if (participant.Weight <= 0 || participant.Weight > 300)
                    errors.Add($"Строка {i + 1}: Вес должен быть от 0.1 до 300 кг");

                if (participant.Birthday > DateTime.Now || participant.Birthday < DateTime.Now.AddYears(-100))
                    errors.Add($"Строка {i + 1}: Некорректная дата рождения");
            }

            return errors;
        }

        [RelayCommand]
        private void ImportParticipants()
        {
            // TODO: Реализация импорта участников
            MessageBox.Show("Функция импорта участников будет реализована позже", "Импорт");
        }

        [RelayCommand]
        private void ExportParticipants()
        {
            // TODO: Реализация экспорта участников
            MessageBox.Show("Функция экспорта участников будет реализована позже", "Экспорт");
        }

        [RelayCommand]
        private void ManageCategories()
        {
            // TODO: Реализация управления категориями
            MessageBox.Show("Функция управления категориями будет реализована позже", "Категории");
        }

        [RelayCommand]
        private void AddParticipant()
        {
            // TODO: Реализация добавления участника
            MessageBox.Show("Функция добавления участника будет реализована позже", "Добавление участника");
        }

        [RelayCommand]
        private void Cancel()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Есть несохраненные изменения. Вы уверены, что хотите выйти?",
                    "Несохраненные изменения",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            _mainViewModel.NavigateCommand.Execute("Tournaments");
        }

        public void MarkAsChanged()
        {
            HasUnsavedChanges = true;
        }

        public void AddNewParticipant()
        {
            Participants.Add(new ParticipantDto
            {
                Id = 0,
                Name = "Новый",
                Surname = "Участник",
                Gender = 1,
                Birthday = DateTime.Now.AddYears(-20),
                Weight = 70.0m
            });
            HasUnsavedChanges = true;
        }
    }
}
