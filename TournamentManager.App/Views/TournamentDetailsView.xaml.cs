using System.Globalization;
using System.Windows.Controls;
using TournamentManager.Client.ViewModels;

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

                if (decimal.TryParse(newText.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, 
                    out decimal result))
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
                if (!decimal.TryParse(text.Replace(',', '.'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal result) || result < 0 || result > 300)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
