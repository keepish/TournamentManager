using System.Windows;
using System.Windows.Controls;

namespace TournamentManager.Client.Views
{
    public partial class WindowControls : UserControl
    {
        public WindowControls()
        {
            InitializeComponent();
            Loaded += WindowControls_Loaded;
        }

        private void WindowControls_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.StateChanged += Window_StateChanged;
                UpdateMaximizeRestoreIcon(window.WindowState);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Close();
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            var window = sender as Window;
            if (window != null)
                UpdateMaximizeRestoreIcon(window.WindowState);
        }

        private void UpdateMaximizeRestoreIcon(WindowState state)
        {
            if (state == WindowState.Maximized)
            {
                MaximizeRestoreIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
            }
            else
            {
                MaximizeRestoreIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
            }
        }
    }
}