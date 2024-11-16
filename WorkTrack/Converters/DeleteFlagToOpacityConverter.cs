using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTrack.Converters
{
    public class DeleteFlagToOpacityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool deleteFlag)
            {
                return deleteFlag ? 0.5 : 1.0;
            }
            return 1.0;
        }
    }
}
