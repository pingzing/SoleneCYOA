using Solene.SimpleParser;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;

namespace Solene.AdminClient.Services
{
    public class ParsingService
    {
        public IEnumerable<Run> FormatBody(string text)
        {
            var runs = new List<Run>();
            var spans = Parser.ApplyRulesToText(text);
            if (!spans.Any())
            {
                return new[] { new Run() { Text = text } };
            }

            int currentIndex = 0;
            foreach (var span in spans)
            {
                if (currentIndex < span.StartIndex)
                {
                    runs.Add(new Run
                    {
                        Text = text.Substring(currentIndex, span.StartIndex - currentIndex),
                    });
                }

                runs.Add(new Run
                {
                    Text = StripUnprintableCharacters(span.Value),
                    FontWeight = span.RuleType == RuleType.Bold
                        ? FontWeights.Bold
                        : FontWeights.Normal,
                    FontStyle = span.RuleType == RuleType.Italic
                        ? FontStyle.Italic
                        : FontStyle.Normal,
                });

                int indexDiff = text.Length - currentIndex;
                if (indexDiff > 0)
                {
                    runs.Add(new Run
                    {
                        Text = text.Substring(currentIndex, indexDiff)
                    });
                }
            }

            return runs;
        }

        private static Regex _unprintables = new Regex(@"[\*_]", RegexOptions.Compiled);
        private static string StripUnprintableCharacters(string text)
        {
            return _unprintables.Replace(text, "");
        }
    }
}


