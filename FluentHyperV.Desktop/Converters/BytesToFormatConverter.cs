using System.Globalization;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class BytesToFormatConverter : ValueConverterBase<IConvertible?, string>
{
    protected override string Convert(IConvertible? value)
    {
        if (value == null)
            return "0 B";
        var bytes = value.ToDouble(CultureInfo.InvariantCulture);
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        var order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes /= 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }
}
