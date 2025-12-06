using System.Windows;

namespace TournamentManager.Client.Views
{
    public partial class GuestWindow : Window
    {
        public GuestWindow()
        {
            InitializeComponent();
        }

        public void SetViewModel(object vm)
        {
            DataContext = vm;
        }
    }
}