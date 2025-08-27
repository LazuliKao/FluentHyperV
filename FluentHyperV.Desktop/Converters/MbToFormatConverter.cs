using System.Globalization;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Converters;

public class MbToFormatConverter : ValueConverterBase<IConvertible?, string>
{
    protected override string Convert(IConvertible? value)
    {
        if (value == null)
            return "0 MB";
        var bytes = value.ToDouble(CultureInfo.InvariantCulture);
        string[] sizes = ["KB", "MB", "GB", "TB"];
        var order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes /= 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }
}