using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FluentHyperV.Desktop.Converters
{
    public class BoolToPaddingConverterForText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasPadding && hasPadding)
            {
                return new Thickness(0, 0, 0, 20);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
