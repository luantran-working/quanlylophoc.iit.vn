using System;
using System.Globalization;
using System.Windows;
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

    /// <summary>
    /// Converts count > 0 to Visible, 0 to Collapsed
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts count > 0 to Collapsed, 0 to Visible (inverse of CountToVisibilityConverter)
    /// </summary>
    public class CountToInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null or empty string to Collapsed, otherwise Visible
    /// </summary>
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            
            if (value is string str && string.IsNullOrWhiteSpace(str))
                return Visibility.Collapsed;
            
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a full name to initials (e.g., "Nguyen Van A" -> "NA")
    /// </summary>
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name && !string.IsNullOrWhiteSpace(name))
            {
                var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    // Take first letter of first and last name
                    return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
                }
                else if (parts.Length == 1)
                {
                    // Just take first two letters or first letter
                    return parts[0].Length >= 2 
                        ? parts[0].Substring(0, 2).ToUpper() 
                        : parts[0].ToUpper();
                }
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}