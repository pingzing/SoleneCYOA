using System;
using System.Globalization;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Converters
{
    public class QuestioSelectableToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLocked = (bool)value;
            if (isLocked)
            {
                return Color.FromHex("33FFFFFF");                
            }
            else
            {
                return Color.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
