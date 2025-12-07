using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TournamentManager.Client.Converters
{
    public class RoundToLeftVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int round)
            {
                // Hide left arrow in first round
                return round <= 1 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
