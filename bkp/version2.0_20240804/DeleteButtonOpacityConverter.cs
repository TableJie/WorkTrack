using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkTrack
{
    public class DeleteButtonOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool deleteFlag)
            {
                return deleteFlag ? 0.3 : 1.0; // 已刪除的顏色變灰
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
