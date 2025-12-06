using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.DTOs.Users;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class CategoriesManagementViewModel : ObservableObject
    {
        private readonly IService<CategoryDto> _categoryService;
        private readonly TournamentDto _tournament;
        private readonly ITournamentCategoryService _tournamentCategoryService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private ObservableCollection<CategoryDto> categories = new();

        [ObservableProperty]
        private ObservableCollection<CategoryDto> attachedCategories = new();

        [ObservableProperty]
        private CategoryDto selectedAttachedCategory;

        [ObservableProperty]
        private ObservableCollection<UserDto> judges = new();

        [ObservableProperty]
        private CategoryDto selectedCategory = new();

        [ObservableProperty]
        private UserDto selectedJudge;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private int sitesNumber = 1;

        public string TournamentName => _tournament?.Name ?? "Турнир";
        public string WindowTitle => $"Управление категориями - {TournamentName}";

        public CategoriesManagementViewModel(TournamentDto tournament, IService<CategoryDto> categoryService,
            ITournamentCategoryService tournamentCategoryService, IUserService userService)
        {
            _tournament = tournament;
            _categoryService = categoryService;
            _tournamentCategoryService = tournamentCategoryService;
            _userService = userService;

            InitializeCategory();
            LoadCategories();
            LoadJudges();
            LoadAttachedCategories();
        }

        [RelayCommand]
        private async Task LoadJudges()
        {
            IsLoading = true;
            try
            {
                var all = await _userService.GetAllAsync() ?? new List<UserDto?>();
                var allUsers = all.Where(u => u != null).Select(u => u!).ToList();

                var organizers = await _userService.GetOrganizersAsync() ?? new List<UserDto?>();
                var organizerIds = organizers.Where(o => o != null).Select(o => o!.Id).ToHashSet();

                var candidates = allUsers
                    .Where(u => !organizerIds.Contains(u.Id))
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .OrderBy(u => u.Surname)
                    .ThenBy(u => u.Name)
                    .ThenBy(u => u.Patronymic)
                    .ToList();

                Judges.Clear();
                foreach (var user in candidates)
                    Judges.Add(user);

                if (Judges.Any())
                    SelectedJudge = Judges.First();
                else
                    MessageBox.Show("Нет доступных пользователей для назначения судьёй (пользователи отсутствуют в таблице Tournament)", "Информация");
            }
            catch
            {
                MessageBox.Show($"Ошибка загрузки списка пользователей", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadCategories()
        {
            IsLoading = true;

            try
            {
                var categoriesList = await _categoryService.GetAllAsync();

                Categories.Clear();
                if (categoriesList != null)
                {
                    foreach (var category in categoriesList)
                        if (category != null)
                            Categories.Add(category);
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка загрузки категорий", "Ошибка");
            }
            finally 
            { 
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadAttachedCategories()
        {
            try
            {
                var attacheddCategoriesList = await _tournamentCategoryService.GetCategoriesByTournamentIdAsync(_tournament.Id);

                AttachedCategories.Clear();
                if (attacheddCategoriesList != null)
                    foreach(var category in attacheddCategoriesList)
                        if (category != null)
                            AttachedCategories.Add(category);
            }
            catch
            {
                MessageBox.Show($"Ошибка загрузки прикрепленных категорий", "Ошибка");
            }
        }

        [RelayCommand]
        private async Task AttachCategory()
        {
            if (SelectedCategory?.Id == 0)
            {
                MessageBox.Show("Выберите категорию для прикрепления", "Внимание");
                return;
            }

            if (AttachedCategories.Any(c => c.Id == SelectedCategory?.Id))
            {
                MessageBox.Show("Эта категория уже прикреплена к турниру", "Внимание");
                return;
            }

            if (SelectedJudge == null)
            {
                MessageBox.Show("Выберите судью для категории", "Внимание");
                return;
            }

            try
            {
                var success = await _tournamentCategoryService.AttachCategoryToTournamentAsync(_tournament.Id, SelectedCategory.Id, SelectedJudge.Id, SitesNumber);

                if (success)
                {
                    await LoadAttachedCategories();
                    MessageBox.Show("Категория успешно прикреплена к турниру", "Успех");

                    SitesNumber = 1;
                    if (Judges.Any())
                        SelectedJudge = Judges.First();

                    SelectedAttachedCategory = null;
                }
                else
                {
                    MessageBox.Show("Ошибка при прикреплении категории", "Ошибка");
                }
            }
            catch
            {
                MessageBox.Show($"Ошибка при прикреплении категории", "Ошибка");
            }
        }

        [RelayCommand]
        private async Task DetachCategory()
        {
            if (SelectedAttachedCategory?.Id == 0 || SelectedAttachedCategory == null)
            {
                MessageBox.Show("Выберите категорию для открепления", "Внимание");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите открепить категорию \"{SelectedAttachedCategory.DisplayName}\" от турнира?",
                "Подтверждение открепления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) 
            {
                try
                {
                    var succes = await _tournamentCategoryService.DetachCategoryFromTournamentAsync(_tournament.Id, SelectedAttachedCategory.Id);

                    if (succes)
                    {
                        await LoadAttachedCategories();
                        SelectedAttachedCategory = null;
                        MessageBox.Show("Категория успешно откреплена от турнира", "Успех");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при откреплении категории", "Ошибка");
                    }
                }
                catch
                {
                    MessageBox.Show($"Ошибка при откреплении категории", "Ошибка");
                }
            }
        }

        [RelayCommand]
        private void AddCategory()
        {
            IsEditMode = true;
            InitializeCategory();
        }

        [RelayCommand]
        private void EditCategory()
        {
            if (SelectedCategory?.Id == 0 || SelectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для редактирования", "Внимание");
                return;
            }

            IsEditMode = true;
        }

        [RelayCommand]
        private async Task SaveCategory()
        {
            if (!ValidateCategory())
                return;

            IsLoading = true;

            try
            {
                if (SelectedCategory.Id == 0)
                {
                    await _categoryService.AddAsync(SelectedCategory);
                    MessageBox.Show("Категория успешно создана!", "Успех");
                }
                else
                {
                    await _categoryService.UpdateAsync(SelectedCategory);
                    MessageBox.Show("Категория успешно обновлена!", "Успех");
                }

                isEditMode = false;
                await LoadCategories();
                InitializeCategory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения категории", "Ошибка");
            }
            finally
            {
                isLoading = false;
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditMode = false;
            InitializeCategory();
        }

        [RelayCommand]
        private async Task DeleteCategory()
        {
            if (SelectedCategory?.Id == 0 || SelectedCategory == null)
            {
                MessageBox.Show("Выберите категорию для удаления", "Внимание");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить категорию \"{SelectedCategory.DisplayName}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _categoryService.DeleteAsync(SelectedCategory.Id);
                    MessageBox.Show("Категория успешно удалена", "Успех");
                    await LoadCategories();
                    InitializeCategory();
                }
                catch (Exception ex)
                {
                        MessageBox.Show($"Ошибка удаления категории", "Ошибка");
                }
            }
        }

        [RelayCommand]
        private void Close()
        {
            CloseWindow();
        }
        private void InitializeCategory()
        {
            SelectedCategory = new CategoryDto
            {
                MinWeight = 50,
                MaxWeight = 60,
                MinAge = 18,
                MaxAge = 35
            };
            SelectedAttachedCategory = null;
        }

        private bool ValidateCategory()
        {
            if (SelectedCategory.MinWeight >= SelectedCategory.MaxWeight)
            {
                MessageBox.Show("Максимальный вес должен быть больше минимального", "Ошибка валидации");
                return false;
            }

            if (SelectedCategory.MinAge >= SelectedCategory.MaxAge)
            {
                MessageBox.Show("Максимальный возраст должен быть больше минимального", "Ошибка валидации");
                return false;
            }

            if (SelectedCategory.MinWeight < 0 || SelectedCategory.MaxWeight > 300)
            {
                MessageBox.Show("Вес должен быть в диапазоне от 0 до 300 кг", "Ошибка валидации");
                return false;
            }

            if (SelectedCategory.MinAge < 0 || SelectedCategory.MaxAge > 100)
            {
                MessageBox.Show("Возраст должен быть в диапазоне от 0 до 100 лет", "Ошибка валидации");
                return false;
            }

            return true;
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows) 
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

    }
}
