using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FluentHyperV.Desktop.Converters;

public class VmStateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string state)
        {
            return state?.ToLower() switch
            {
                "running" => new SolidColorBrush(Color.FromRgb(34, 197, 94)), // 绿色 - 运行中
                "off" => new SolidColorBrush(Color.FromRgb(156, 163, 175)), // 灰色 - 已关闭
                "saved" => new SolidColorBrush(Color.FromRgb(59, 130, 246)), // 蓝色 - 已保存
                "paused" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // 橙色 - 已暂停
                "starting" => new SolidColorBrush(Color.FromRgb(16, 185, 129)), // 青绿色 - 启动中
                "stopping" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // 红色 - 停止中
                "pausing" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // 橙色 - 暂停中
                "resuming" => new SolidColorBrush(Color.FromRgb(16, 185, 129)), // 青绿色 - 恢复中
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128)) // 默认灰色
            };
        }

        return new SolidColorBrush(Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
