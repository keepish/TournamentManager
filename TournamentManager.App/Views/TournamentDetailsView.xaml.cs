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
            if (DataContext is TournamentDetailsViewModel viewModel)
            {
                viewModel.MarkAsChanged();
            }
        }

        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (DataContext is TournamentDetailsViewModel viewModel)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                    viewModel.MarkAsChanged()
                ), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (DataContext is TournamentDetailsViewModel viewModel)
            {
                viewModel.MarkAsChanged();
            }
        }
    }
}
