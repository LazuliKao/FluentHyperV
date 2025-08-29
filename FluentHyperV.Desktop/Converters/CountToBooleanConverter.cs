using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters;

public class CountToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }

        // 如果值不是 null，则返回 true
        return value is not null;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
