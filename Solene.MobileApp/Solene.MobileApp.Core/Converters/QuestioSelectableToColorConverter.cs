using System;
using System.Globalization;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Converters
{
    public class QuestioSelectableToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelectable = (bool)value;
            if (isSelectable)
            {
                return Color.Transparent;
            }
            else
            {
                return Color.FromHex("33FFFFFF");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
