using Wpf.Ui.Controls;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperVDesktop.Converters;

public class BoolToAppearanceConverter : ValueConverterBase<bool, ControlAppearance>
{
    protected override ControlAppearance ConvertNonNullValue(bool value)
    {
        return value ? ControlAppearance.Secondary : ControlAppearance.Primary;
    }
}
