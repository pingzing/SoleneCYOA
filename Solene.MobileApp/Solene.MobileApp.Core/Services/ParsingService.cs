using Solene.SimpleParser;
using System.Linq;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Services
{
    public interface IParsingService
    {
        FormattedString FormatBody(string text);
    }

    public class ParsingService : IParsingService
    {
        public FormattedString FormatBody(string text)
        {
            FormattedString formattedString = new FormattedString();
            foreach(var span in Parser.ArrangeText(text)
                .Select(x => new Span
                {
                    Text = x.Text,
                    FontAttributes = (FontAttributes)x.Formatting,
                }))
            {
                formattedString.Spans.Add(span);
            }

            return formattedString;
        }

    }
}
