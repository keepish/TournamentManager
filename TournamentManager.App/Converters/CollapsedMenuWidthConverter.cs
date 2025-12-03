using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TournamentManager.Client.Converters
{
    public class CollapsedMenuWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCollapsed)
            {
                return isCollapsed ? new GridLength(90) : new GridLength(200);
            }
            return new GridLength(200);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}