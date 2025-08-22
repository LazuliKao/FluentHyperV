using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class BoolToInvertedBoolConverter : ValueConverterBase<bool, bool>
{
    protected override bool ConvertNonNullValue(bool value)
    {
        return !value;
    }
}
