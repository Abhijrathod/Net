using System;
using System.Globalization;
using System.Windows.Data;

namespace DNSChanger.Converters
{
    public class TestButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTesting)
            {
                return isTesting ? "Testing..." : "Run Speed Test";
            }
            return "Run Speed Test";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
