using GalaSoft.MvvmLight.Ioc;
using Solene.MobileApp.Core.Services;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Converters
{
    public class QuestionBodyConverter : IValueConverter
    {
        private static IParsingService _parsingService;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_parsingService == null)
            {
                _parsingService = SimpleIoc.Default.GetInstance<IParsingService>();
            }

            string body = (string)value;
            return _parsingService.FormatBody(body);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
