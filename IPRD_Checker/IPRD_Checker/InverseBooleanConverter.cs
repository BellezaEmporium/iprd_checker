using System;
using System.Globalization;
using System.Windows.Data;

namespace IPRD_Checker
{
    // Token: 0x0200000B RID: 11
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        // Token: 0x06000053 RID: 83 RVA: 0x00002E54 File Offset: 0x00001054
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = false;
            bool flag;
            if (value != null)
            {
                if (value is bool b)
                {
                    boolean = b;
                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            else
            {
                flag = false;
            }
            var flag2 = flag;
            object obj;
            if (flag2)
            {
                obj = !boolean;
            }
            else
            {
                obj = true;
            }
            return obj;
        }

        // Token: 0x06000054 RID: 84 RVA: 0x00002E96 File Offset: 0x00001096
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
