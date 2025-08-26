using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string parameterString && int.TryParse(parameterString, out int targetValue))
        {
            return intValue == targetValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter is string parameterString && int.TryParse(parameterString, out int targetValue))
        {
            return targetValue;
        }
        return Binding.DoNothing;
    }
}
