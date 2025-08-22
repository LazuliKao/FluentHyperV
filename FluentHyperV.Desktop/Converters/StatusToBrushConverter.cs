using System.Windows.Media;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class StatusToBrushConverter : ValueConverterBase<bool, Brush>
{
    public Brush TrueValue { get; set; } = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E));
    public Brush FalseValue { get; set; } = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));

    protected override Brush ConvertNonNullValue(bool value)
    {
        return value ? TrueValue : FalseValue;
    }
}
