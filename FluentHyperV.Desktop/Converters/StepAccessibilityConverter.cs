using System;
using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters
{
    public class StepAccessibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not int maxCompletedStep || values[1] is not int stepNumber)
                return false;

            // 允许访问已完成的步骤或下一步
            return stepNumber <= maxCompletedStep || stepNumber == maxCompletedStep + 1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
