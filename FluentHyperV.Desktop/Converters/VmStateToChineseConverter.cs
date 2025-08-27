using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters;

public class VmStateToChineseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string state)
        {
            return state?.ToLower() switch
            {
                "running" => "运行中",
                "off" => "已关闭",
                "saved" => "已保存",
                "paused" => "已暂停",
                "starting" => "启动中",
                "stopping" => "停止中",
                "pausing" => "暂停中",
                "resuming" => "恢复中",
                "reset" => "重置中",
                "other" => "其他",
                _ => state ?? "未知"
            };
        }

        return "未知";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
