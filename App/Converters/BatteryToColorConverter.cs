using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AegisLink.App.Converters
{
    public class BatteryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int batteryLevel)
            {
                // Critical Level < 20%
                if (batteryLevel < 20)
                {
                    return Brushes.Red;
                }
            }
            // Default Aero-Space Cyan
            return Brushes.Cyan;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
