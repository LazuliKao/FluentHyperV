using WpfExtensions.Xaml.Converters;

namespace FluentHyperVDesktop.Converters;

public class StringToVisibilityConverter : ValueConverterBase<string, Visibility>
{
    protected override Visibility ConvertNonNullValue(string value)
    {
        return string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
    }
}
