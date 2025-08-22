using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class DateTimeToStringConverter : ValueConverterBase<DateTime, string>
{
    protected override string ConvertNonNullValue(DateTime value)
    {
        return value.Year > 1 ? value.ToString("f") : string.Empty;
    }
}
