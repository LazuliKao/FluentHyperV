using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FluentHyperV.Desktop.Converters;

public class StepToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter is string stepParameter && int.TryParse(stepParameter, out int targetStep))
        {
            if (currentStep > targetStep)
            {
                // 已完成的步骤 - 绿色
                return new SolidColorBrush(Color.FromRgb(34, 197, 94));
            }
            else if (currentStep == targetStep)
            {
                // 当前步骤 - 蓝色
                return new SolidColorBrush(Color.FromRgb(59, 130, 246));
            }
            else
            {
                // 未完成的步骤 - 灰色
                return new SolidColorBrush(Color.FromRgb(156, 163, 175));
            }
        }

        return new SolidColorBrush(Color.FromRgb(156, 163, 175));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
