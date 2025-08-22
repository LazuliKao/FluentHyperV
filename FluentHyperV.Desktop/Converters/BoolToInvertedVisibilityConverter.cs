using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class BoolToInvertedVisibilityConverter : ValueConverterBase<bool, Visibility>
{
    protected override Visibility ConvertNonNullValue(bool value)
    {
        return value ? Visibility.Collapsed : Visibility.Visible;
    }
}
