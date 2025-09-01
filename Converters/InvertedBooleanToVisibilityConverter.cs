using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GamingTweaksManager.Converters
{
    /// <summary>
    /// Conversor que inverte o comportamento do BooleanToVisibilityConverter
    /// True = Collapsed, False = Visible
    /// </summary>
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            return false;
        }
    }
}
