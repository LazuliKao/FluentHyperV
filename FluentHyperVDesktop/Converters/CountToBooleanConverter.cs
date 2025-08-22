using System.Globalization;
using System.Windows.Data;

namespace FluentHyperVDesktop.Converters;

public class CountToBooleanConverter : IValueConverter
{
    public static readonly CountToBooleanConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
