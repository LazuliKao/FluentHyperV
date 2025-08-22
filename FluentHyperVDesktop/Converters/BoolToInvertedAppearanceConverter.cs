using Wpf.Ui.Controls;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperVDesktop.Converters;

public class BoolToInvertedAppearanceConverter : ValueConverterBase<bool, ControlAppearance>
{
    protected override ControlAppearance ConvertNonNullValue(bool value)
    {
        return value ? ControlAppearance.Primary : ControlAppearance.Secondary;
    }
}
