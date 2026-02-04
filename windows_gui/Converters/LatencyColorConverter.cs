using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DNSChanger.Converters
{
    public class LatencyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double latency)
            {
                if (latency < 50)
                    return new SolidColorBrush(Color.FromRgb(78, 201, 176)); // Green - Excellent
                else if (latency < 100)
                    return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow - Good
                else if (latency < 200)
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange - Fair
                else
                    return new SolidColorBrush(Color.FromRgb(244, 135, 113)); // Red - Poor
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
