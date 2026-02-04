using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DNSChanger.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue 
                    ? new SolidColorBrush(Color.FromRgb(78, 201, 176)) // SuccessGreen
                    : new SolidColorBrush(Color.FromRgb(244, 135, 113)); // ErrorRed
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
