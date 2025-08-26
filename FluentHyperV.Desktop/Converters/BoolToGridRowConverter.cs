using System;
using System.Globalization;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters
{
    public class BoolToGridRowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFloating && isFloating)
            {
                return 1; // 如果是浮动文本，放在第二行
            }
            return 0; // 否则放在第一行
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
