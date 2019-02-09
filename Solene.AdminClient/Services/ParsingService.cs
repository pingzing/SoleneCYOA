using Solene.SimpleParser;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml.Documents;

namespace Solene.AdminClient.Services
{
    public class ParsingService
    {
        public IEnumerable<Run> FormatBody(string text)
        {
            return Parser.ArrangeText(text)
                .Select(x => new Run
                {
                    Text = x.Text,
                    FontWeight = x.Formatting == RuleType.Bold
                        ? FontWeights.Bold
                        : FontWeights.Normal,
                    FontStyle = x.Formatting == RuleType.Italic
                        ? FontStyle.Italic
                        : FontStyle.Normal
                });
        }
    }
}


