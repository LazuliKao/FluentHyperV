using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class BoolToInvertedVisibilityConverter : ValueConverterBase<object, Visibility>
{
    protected override Visibility Convert(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }
}
