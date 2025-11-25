using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using TournamentManager.Core.DTOs.Tournaments;

namespace TournamentManager.Client.Converters
{
    public class TournamentStatusToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TournamentDto tournament)
            {
                var now = DateTime.Now;

                if (now < tournament.StartDate)
                    return "Предстоящий";
                else if (now >= tournament.StartDate && now <= tournament.EndDate)
                    return "Активный";
                else
                    return "Завершенный";
            }
            return "Неизвестно";
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
