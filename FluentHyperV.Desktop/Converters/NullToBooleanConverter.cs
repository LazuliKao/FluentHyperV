using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters;

/// <summary>
/// 将 null 值转换为 Boolean 值的转换器
/// null -> false, 非 null -> true
/// </summary>
public class NullToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
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
