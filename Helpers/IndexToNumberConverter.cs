using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MahnitolaList.Helpers
{
    public class ItemIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] != null && values[1] is ListBox lb)
            {
                int idx = lb.Items.IndexOf(values[0]);
                if (idx >= 0) return (idx + 1).ToString() + ".";
            }
            return string.Empty;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
