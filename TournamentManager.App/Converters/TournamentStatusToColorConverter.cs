using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using TournamentManager.Core.DTOs.Tournaments;

namespace TournamentManager.Client.Converters
{
    public class TournamentStatusToColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TournamentDto tournament)
            {
                var now = DateTime.Now;

                if (now < tournament.StartDate)
                    return new SolidColorBrush(Colors.Orange);
                else if (now >= tournament.StartDate && now <= tournament.EndDate)
                    return new SolidColorBrush(Colors.Green);
                else
                    return new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
