using WpfExtensions.Xaml.Converters;

namespace FluentHyperVDesktop.Converters;

public class DateTimeToStringConverter : ValueConverterBase<DateTime, string>
{
    protected override string ConvertNonNullValue(DateTime value)
    {
        return value.Year > 1 ? value.ToString("f") : string.Empty;
    }
}
