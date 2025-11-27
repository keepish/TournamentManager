using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using TournamentManager.Client.Views;
using TournamentManager.Core.DTOs.Categories;
using TournamentManager.Core.DTOs.Participants;
using TournamentManager.Core.DTOs.Tournaments;
using TournamentManager.Core.Services;
using Excel = Microsoft.Office.Interop.Excel;

namespace TournamentManager.Client.ViewModels
{
    public partial class TournamentDetailsViewModel : ObservableObject
    {
        private readonly TournamentDto _tournament;
        private readonly ApiService _apiService;
        private readonly MainViewModel _mainViewModel;
        private readonly IService<CategoryDto> _categoryService;

        [ObservableProperty]
        private ObservableCollection<ParticipantDto> participants = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        public bool IsOrganizer => _mainViewModel.CurrentUser.IsOrganizer;

        public string TournamentName => _tournament?.Name ?? "Детали турнира";
        public string TournamentDates => $"{_tournament?.StartDate:dd.MM.yyyy} - {_tournament?.EndDate:dd.MM.yyyy}";
        public string TournamentAddress => _tournament?.Address ?? "";
        public string TournamentDescription => _tournament?.Description ?? "";

        public TournamentDetailsViewModel(TournamentDto tournament, ApiService apiService,
            MainViewModel mainViewModel, IService<CategoryDto> categoryService) 
        {
            _tournament = tournament;
            _apiService = apiService;
            _mainViewModel = mainViewModel;
            _categoryService = categoryService;

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

                if (IsOrganizer)
                {
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
            if (!IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут регистрировать участников.", "Ошибка доступа");
                return;
            }

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
        private async Task ImportParticipants()
        {
            if (!IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут импортировать участников.", "Ошибка доступа");
                return;
            }

            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
                    Title = "Выберите файл для импорта участников"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    var importedParticipants = await ParseExcelFile(openFileDialog.FileName);
                    
                    if (importedParticipants.Any())
                    {
                        foreach (var participant in importedParticipants)
                            Participants.Add(participant);

                        HasUnsavedChanges = true;
                        MessageBox.Show($"Успешно импортировано {importedParticipants.Count} участников", "Импорт завершен");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось импортировать участников из файла", "Ошибка импорта");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте участников: {ex.Message}", "Ошибка импорта");
            }
            finally 
            {
                IsLoading = false;
            }
        }

        private async Task<List<ParticipantDto>> ParseExcelFile(string filePath)
        {
            var participants = new List<ParticipantDto>();

            try
            {
                var excelApp = new Excel.Application();
                Excel.Workbook workbook = null;
                Excel.Worksheet worksheet = null;

                try
                {
                    workbook = excelApp.Workbooks.Open(filePath);
                    worksheet = workbook.Sheets[1];

                    var usedRange = worksheet.UsedRange;
                    var rowCount = usedRange.Rows.Count;
                    var colCount = usedRange.Columns.Count;

                    if (rowCount < 2) return participants;

                    var headers = new Dictionary<string, int>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Value?.ToString()?.ToLower().Trim();

                        if (!string.IsNullOrEmpty(headerValue))
                            headers[headerValue] = col;
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var participant = ParseParticipantFromRow(worksheet, row, headers);

                            if (participant != null && !string.IsNullOrWhiteSpace(participant.Name) && !string.IsNullOrWhiteSpace(participant.Surname))
                                participants.Add(participant);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка парсинга строки {row}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    if (workbook != null)
                    {
                        workbook.Close(false);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                    }
                    if (worksheet != null)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с Excel: {ex.Message}\n\nУбедитесь, что Excel установлен на компьютере.", "Ошибка Excel");
            }

            return participants;
        }

        private ParticipantDto ParseParticipantFromRow(Worksheet worksheet, int row, Dictionary<string, int> headers)
        {
            var participant = new ParticipantDto();

            if (headers.ContainsKey("имя"))
                participant.Name = GetCellValue(worksheet, row, headers["имя"])?.Trim() ?? "";

            if (headers.ContainsKey("фамилия"))
                participant.Surname = GetCellValue(worksheet, row, headers["фамилия"])?.Trim() ?? "";

            if (headers.ContainsKey("отчество"))
                participant.Patronymic = GetCellValue(worksheet, row, headers["отчество"]).Trim();

            if (headers.ContainsKey("телефон"))
            {
                var phone = GetCellValue(worksheet, row, headers["телефон"])?.Trim();
                if (!string.IsNullOrEmpty(phone))
                    participant.Phone = new string(phone.Where(char.IsDigit).ToArray());
            }    

            if (headers.ContainsKey("пол"))
            {
                var genderValue = GetCellValue(worksheet, row, headers["пол"])?.Trim();

                if (!string.IsNullOrEmpty(genderValue))
                {
                    if (genderValue == "1" || genderValue.ToLower() == "мужской" || genderValue.ToLower() == "м")
                        participant.Gender = 1;
                    else if (genderValue == "0" || genderValue.ToLower() == "женский" || genderValue.ToLower() == "ж")
                        participant.Gender = 2;
                }
            }

            if (headers.ContainsKey("вес"))
            {
                var weightValue = GetCellValue(worksheet, row, headers["вес"])?.Trim();
                if (!string.IsNullOrEmpty(weightValue) && decimal.TryParse(weightValue.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out decimal weight))
                    participant.Weight = weight;
            }

            if (headers.ContainsKey("дата рождения"))
            {
                var birthdayValue = worksheet.Cells[row, headers["дата рождения"]].Value;
                DateTime birthday = DateTime.MinValue;

                if (birthdayValue is DateTime dateTimeValue)
                    birthday = dateTimeValue;
                else
                {
                    var stringValue = birthdayValue?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        if (DateTime.TryParseExact(stringValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday) ||
                            DateTime.TryParseExact(stringValue, "dd.MM.yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday) ||
                            DateTime.TryParse(stringValue, out birthday))
                        {

                        }
                    }
                }

                if (birthday != DateTime.MinValue && birthday.Year > 1900)
                {
                    participant.Birthday = birthday;
                }
            }

            return participant;
        }

        private string GetCellValue(Worksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Value?.ToString();
        }

        [RelayCommand]
        private async Task ExportParticipants()
        {
            if (!IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут экспортировать участников.", "Ошибка доступа");
                return;
            }

            try
            {
                if (!Participants.Any())
                {
                    MessageBox.Show("Нет участников для экспорта", "Экспорт");
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Сохранить файл с участниками",
                    FileName = $"Участники_турнира_{DateTime.Now:yyyy-MM-dd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;

                    await Task.Run(() => ExportToExcel(saveFileDialog.FileName));

                    MessageBox.Show($"Участники успешно экспортированы в файл: {saveFileDialog.FileName}", "Экспорт завершен");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте участников: {ex.Message}", "Ошибка экспорта");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExportToExcel(string filePath)
        {
            Excel.Application excelApp = null;
            Workbook workbook = null;
            Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                workbook = excelApp.Workbooks.Add();
                worksheet = (Worksheet)workbook.Sheets[1];
                worksheet.Name = "Участники турнира";

                string[] headers = { "Имя", "Фамилия", "Отчество", "Телефон", "Пол", "Вес (кг)", "Дата рождения"};

                for (int col = 0; col < headers.Length; col++)
                    worksheet.Cells[1, col + 1] = headers[col];

                Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, headers.Length]];
                headerRange.Font.Bold = true;
                headerRange.Font.Size = 12;
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                headerRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                headerRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                headerRange.Borders.Weight = XlBorderWeight.xlThin;

                for (int i = 0; i < Participants.Count; i++)
                {
                    var participant = Participants[i];
                    int row = i + 2;

                    worksheet.Cells[row, 1] = participant.Name;
                    worksheet.Cells[row, 2] = participant.Surname;
                    worksheet.Cells[row, 3] = participant.Patronymic ?? "-";
                    worksheet.Cells[row, 4] = participant.Phone ?? "-";
                    worksheet.Cells[row, 5] = participant.GenderDisplay;

                    Excel.Range weightCell = (Excel.Range)worksheet.Cells[row, 6];
                    weightCell.Value = (double)participant.Weight;

                    Excel.Range dateCell = (Excel.Range)worksheet.Cells[row, 7];
                    dateCell.Value = participant.Birthday;

                    dateCell.NumberFormat = "dd.mm.yyyy";

                    weightCell.NumberFormat = "0.0";
                }

                if (Participants.Count > 0)
                {
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[Participants.Count + 1, headers.Length]];
                    dataRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                    dataRange.Borders.Weight = XlBorderWeight.xlThin;

                    Excel.Range weightRange = worksheet.Range[worksheet.Cells[2, 6], worksheet.Cells[Participants.Count + 1, 6]];
                    weightRange.HorizontalAlignment = XlHAlign.xlHAlignRight;
                }

                worksheet.Columns.AutoFit();

                worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, headers.Length]].AutoFilter();

                workbook.SaveAs(filePath);

                MessageBox.Show($"Файл успешно сохранен: {System.IO.Path.GetFileName(filePath)}\n\nЭкспортировано участников: {Participants.Count}", "Экспорт завершен");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Excel файла: {ex.Message}", ex);
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }
                if (worksheet != null)
                {
                    Marshal.ReleaseComObject(worksheet);
                }
            }
        }

        [RelayCommand]
        private void ManageCategories()
        {
            if (!IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут управлять категориями.", "Ошибка доступа");
                return;
            }

            var categoriesWindow = new CategoriesManagementWindow
            {
                Owner = System.Windows.Application.Current.MainWindow,
                DataContext = new CategoriesManagementViewModel(_tournament, _categoryService)
            };

            categoriesWindow.ShowDialog();
        }

        [RelayCommand]
        private void AddParticipant()
        {
            if (!IsOrganizer)
            {
                MessageBox.Show("Доступ запрещен. Только организаторы могут добавлять участников.", "Ошибка доступа");
                return;
            }

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
