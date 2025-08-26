using Wpf.Ui.Controls;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class BoolToAppearanceConverter : ValueConverterBase<bool, ControlAppearance>
{
    protected override ControlAppearance ConvertNonNullValue(bool value)
    {
        return value ? ControlAppearance.Primary : ControlAppearance.Secondary;
    }
}
