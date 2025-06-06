using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace WordForge.Services
{
    [ValueConversion(typeof(string), typeof(string))]
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
                return Path.GetFileName(path);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
