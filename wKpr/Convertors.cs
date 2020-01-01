using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace wKpr
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility retTrue = Visibility.Visible;
            Visibility retFalse = Visibility.Collapsed;
            if (parameter is string)
            {
                string strParam = parameter as string;
                if (strParam == "true" || strParam == "Visible")
                {
                    retTrue = Visibility.Visible;
                    retFalse = Visibility.Collapsed;
                }
                else if (strParam == "false" || strParam == "Collapsed")
                {
                    retTrue = Visibility.Collapsed;
                    retFalse = Visibility.Visible;
                }
            }

            Visibility ret;
            bool? isVisible = value as bool?;
            if (isVisible.Value == true)
                ret= retTrue;
            else
                ret= retFalse;

            Debug.WriteLine($"BoolToVisibilityConverter value={value}, Parameter = {parameter}, return={ret}");
            return ret;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? visibility = value as Visibility?;
            return visibility == Visibility.Visible;
        }
    }
  
}
