using WpfExtensions.Xaml.Converters;

namespace FluentHyperVDesktop.Converters;

public class BoolToInvertedBoolConverter : ValueConverterBase<bool, bool>
{
    protected override bool ConvertNonNullValue(bool value)
    {
        return !value;
    }
}
