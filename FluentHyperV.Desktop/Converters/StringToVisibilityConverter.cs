using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class StringToVisibilityConverter : ValueConverterBase<string, Visibility>
{
    protected override Visibility ConvertNonNullValue(string value)
    {
        return string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
    }
}
