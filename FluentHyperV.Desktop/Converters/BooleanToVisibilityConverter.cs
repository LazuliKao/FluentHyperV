using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class BooleanToVisibilityConverter : ValueConverterBase<bool, Visibility>
{
    protected override Visibility ConvertNonNullValue(bool value)
    {
        return value ? Visibility.Visible : Visibility.Collapsed;
    }
}
