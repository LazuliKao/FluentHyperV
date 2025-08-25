using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters;

public class GenerationToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int generation)
        {
            return generation - 1; // Generation 1 -> Index 0, Generation 2 -> Index 1
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index + 1; // Index 0 -> Generation 1, Index 1 -> Generation 2
        }
        return 1;
    }
}
