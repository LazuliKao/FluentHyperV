using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class GenerationToBoolConverter : ValueConverterBase<int, bool>
{
    protected override bool ConvertNonNullValue(int value)
    {
        return value == 2; // 只有第二代支持安全启动
    }
}
