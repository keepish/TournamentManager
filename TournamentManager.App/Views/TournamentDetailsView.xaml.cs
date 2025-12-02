using System.Globalization;
using System.Windows.Controls;
using TournamentManager.Client.ViewModels;
using System.Windows.Input;
using System.Windows;
using TournamentManager.Core.DTOs.Participants;
using System.Linq;

namespace TournamentManager.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для TournamentDetailsView.xaml
    /// </summary>
    public partial class TournamentDetailsView : UserControl
    {
        public TournamentDetailsView()
        {
            InitializeComponent();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataContext is TournamentDetailsViewModel viewModel && e.EditAction == DataGridEditAction.Commit)
            {
                viewModel.MarkAsChanged();
            }
        }

        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            // запрещаем добавление через DataGrid напрямую, используется отдельная кнопка
            e.NewItem = null;
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (DataContext is TournamentDetailsViewModel viewModel && e.EditAction == DataGridEditAction.Commit)
            {
                viewModel.MarkAsChanged();
            }
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            string newText = textBox?.Text + e.Text;

            if (!string.IsNullOrEmpty(e.Text))
            {
                foreach (char c in e.Text)
                {
                    if (!char.IsDigit(c) && c != '.' && c != ',')
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if (decimal.TryParse(newText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    if (result < 0 || result > 300)
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void TextBox_Pasting(object sender, System.Windows.DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!decimal.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) || result < 0 || result > 300)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private async void ParticipantsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && DataContext is TournamentDetailsViewModel vm)
            {
                var grid = sender as DataGrid;
                var toDelete = grid?.SelectedItems
                    .OfType<ParticipantDto>()
                    .Where(IsRealParticipant)
                    .ToList();
                if (toDelete != null && toDelete.Count > 0)
                {
                    e.Handled = true; // предотвращаем стандартное удаление
                    if (ConfirmDelete(toDelete.Count))
                    {
                        foreach (var p in toDelete.ToList())
                        {
                            await vm.DeleteParticipantAsync(p);
                        }
                    }
                }
            }
        }

        private bool ConfirmDelete(int count)
        {
            var message = count == 1 ? "Удалить выбранного участника?" : $"Удалить выбранных участников ({count})?";
            return MessageBox.Show(message, "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = ParticipantsGrid; // поле, сгенерированное из x:Name в XAML
            if (DataContext is TournamentDetailsViewModel vm && grid != null && grid.SelectedItems.Count > 0)
            {
                var toDelete = grid.SelectedItems
                    .OfType<ParticipantDto>()
                    .Where(IsRealParticipant)
                    .ToList();
                if (toDelete.Count == 0)
                {
                    MessageBox.Show("Нет выбранных участников для удаления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (ConfirmDelete(toDelete.Count))
                {
                    foreach (var p in toDelete.ToList())
                    {
                        await vm.DeleteParticipantAsync(p);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите участника(ов) для удаления", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Определяет, что это реальная запись участника, а не пустая строка-плейсхолдер
        private static bool IsRealParticipant(ParticipantDto p)
        {
            if (p == null) return false;
            bool hasName = !string.IsNullOrWhiteSpace(p.Name) || !string.IsNullOrWhiteSpace(p.Surname);
            bool hasData = p.Id > 0 || hasName || p.Weight > 0 || (p.Birthday != default);
            return hasData;
        }
    }
}
