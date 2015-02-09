using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace UB.Converter
{
    
    [ValueConversion(typeof(double), typeof(GridLength))]
    sealed class DoubleToGridLength : IValueConverter
    {
        private GridLengthConverter gridLengthConverter = new GridLengthConverter();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (GridLength)gridLengthConverter.ConvertFrom((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((GridLength)value).Value;
        }
    }
}
