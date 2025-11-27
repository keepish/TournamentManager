using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;

namespace TournamentManager.Client.ViewModels
{
    public partial class CategoriesManagementViewModel : ObservableObject
    {
        private readonly IService<CategoryDto> _categoryService;
        private readonly TournamentDto _tournament;

        [ObservableProperty]
        private ObservableCollection<CategoryDto> categories = new();

        [ObservableProperty]
        private CategoryDto selectedCategory = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isEditMode;

        public string TournamentName => _tournament?.Name ?? "Турнир";
        public string WindowTitle => $"Управление категориями - {TournamentName}";

        public CategoriesManagementViewModel(TournamentDto tournament, IService<CategoryDto> categoryService)
        {
            _tournament = tournament;
            _categoryService = categoryService;

            InitializeCategory();
            LoadCategories();
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
                    {
                        if (category != null)
                            Categories.Add(category);
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка");
            }
            finally 
            { 
                IsLoading = false;
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
            if (SelectedCategory?.Id == 0)
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
                MessageBox.Show($"Ошибка сохранения категории: {ex.Message}", "Ошибка");
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
            if (SelectedCategory?.Id == 0)
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
                         MessageBox.Show($"Ошибка удаления категории: {ex.Message}", "Ошибка");
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
