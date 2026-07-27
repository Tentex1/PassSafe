using System;
using System.Globalization;

namespace PassSafe.Converters
{
    /// <summary>
    /// Converts a string representation of a number to an integer, and vice versa.
    /// Mainly used for Pickers dealing with numerical limits (e.g., Password Length).
    /// </summary>
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
            // Fallback default password length
            return 8;
        }
    }
}