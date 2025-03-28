using System;
using System.Globalization;
using System.Windows.Data;

namespace LocationMontagne.Services
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fileName = value as string;
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            return ImageService.LoadImage(fileName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
