using Solene.AdminClient.Services;
using System;
using Windows.UI.Xaml.Data;

namespace Solene.AdminClient.Converters
{
    public class QuestionBodyConverter : IValueConverter
    {
        private static ParsingService _parsingService;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (_parsingService == null)
            {
                _parsingService = new ParsingService();
            }

            string bodyText = (string)value;
            return _parsingService.FormatBody(bodyText);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
