using System;
using System.Globalization;
using System.Windows.Data;

namespace IPRD_Checker
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class EnableDisableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag;
            if (value != null && value is bool)
            {
                var boolean = (bool)value;
                flag = !boolean;
            }
            else
            {
                flag = true;
            }
            var flag2 = flag;
            object obj;
            if (flag2)
            {
                obj = "#9e9e9e";
            }
            else
            {
                obj = "#000000";
            }
            return obj;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
