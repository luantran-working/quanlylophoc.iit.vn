using System;
using System.Globalization;
using System.Windows.Data;

namespace ClassroomManagement.Converters
{
    /// <summary>
    /// Converts a boolean value to its inverse
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            // Try to parse as boolean
            if (value != null && bool.TryParse(value.ToString(), out bool parsedValue))
            {
                return !parsedValue;
            }
            
            // Default fallback
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            // Try to parse as boolean
            if (value != null && bool.TryParse(value.ToString(), out bool parsedValue))
            {
                return !parsedValue;
            }
            
            // Default fallback
            return false;
        }
    }
}