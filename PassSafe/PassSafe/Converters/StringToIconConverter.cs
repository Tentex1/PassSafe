using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using MauiIcons.Material.Sharp;

namespace PassSafe.Converters
{
    public class StringToIconConverter : IValueConverter
    {

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string iconName && Enum.TryParse(typeof(MaterialSharpIcons), iconName, out var iconEnum))
            {
                return iconEnum;
            }

            // Eğer veritabanından boş gelir veya eşleşmezse varsayılan bir ikon dönsün
            return MaterialSharpIcons.Lock;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}