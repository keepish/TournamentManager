using System.Windows.Controls;

namespace TournamentManager.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для PlaceholderView.xaml
    /// </summary>
    public partial class PlaceholderView : UserControl
    {
        public string Message { get; set; }

        public PlaceholderView()
        {
            InitializeComponent();

            DataContext = this;
        }

        public PlaceholderView(string message) : this()
        {
            Message = $"Функциональность '{message}' находится в разработке";
        }
    }
}
