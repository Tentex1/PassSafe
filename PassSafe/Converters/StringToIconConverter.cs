using System;
using System.Globalization;
using MauiIcons.Material.Sharp;

namespace PassSafe.Converters
{
    /// <summary>
    /// Converts a string representation of a MaterialSharpIcon into its corresponding Enum value.
    /// Allows icons to be stored as strings in the database and rendered properly in XAML.
    /// </summary>
    public class StringToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string iconName && Enum.TryParse(typeof(MaterialSharpIcons), iconName, out var iconEnum))
            {
                return iconEnum;
            }

            // Fallback icon if parsing fails
            return MaterialSharpIcons.Lock;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}