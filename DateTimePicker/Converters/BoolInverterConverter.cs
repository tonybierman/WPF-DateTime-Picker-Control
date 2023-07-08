using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Net.Http.Headers;

namespace DateTimePicker.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if(value != null && value is bool)
                {
                    bool retval = !(bool)value;
                    return retval;
                }

                return DependencyProperty.UnsetValue;
            }
            catch { return DependencyProperty.UnsetValue; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
