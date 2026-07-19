using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PassSafe.Converters
{
    public class StringToIntConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string strValue && int.TryParse(strValue, out int result))
            {
                return result;
            }
            return 8;
        }
    }
}
