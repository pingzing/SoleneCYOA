using Solene.SimpleParser;
using System.Linq;
using System.Text.RegularExpressions;
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
            var formattedString = new FormattedString();
            var spans = Parser.ApplyRulesToText(text);
            if(!spans.Any())
            {
                formattedString.Spans.Add(new Span { Text = text });
                return formattedString;
            }

            int currentIndex = 0;
            foreach(var span in spans)
            {
                //if this rule match is preceded by something that doesn't match a rule, pull it out as regular text.
                if (currentIndex < span.StartIndex)
                {
                    formattedString.Spans.Add(new Span
                    {
                        Text = text.Substring(currentIndex, span.StartIndex - currentIndex)
                    });
                }

                formattedString.Spans.Add(new Span
                {
                    Text = StripUnprintableCharacters(span.Value),
                    FontAttributes = (FontAttributes)span.RuleType
                });
                currentIndex = span.EndIndex;
            }

            int indexDiff = text.Length - currentIndex;
            if (indexDiff > 0)
            {
                formattedString.Spans.Add(new Span
                {
                    Text = text.Substring(currentIndex, indexDiff)
                });
            }
           
            return formattedString;
        }

        private static Regex _unprintables = new Regex(@"[\*_]", RegexOptions.Compiled);
        private static string StripUnprintableCharacters(string text)
        {
            return _unprintables.Replace(text, "");
        }
    }
}
