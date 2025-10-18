using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
