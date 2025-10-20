using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace TournamentManager.Client.Converters
{
    public class StatusToColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Предстоящий" => new SolidColorBrush(Color.FromRgb(25, 118, 210)),
                    "Активный" => new SolidColorBrush(Color.FromRgb(56, 142, 60)),
                    "Завершенный" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    _ => new SolidColorBrush(Color.FromRgb(25, 118, 210))
                };
            }
            return new SolidColorBrush(Color.FromRgb(25, 118, 210));
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
