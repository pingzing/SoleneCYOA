using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Solene.SimpleParser
{
    public class Parser
    {
        private static readonly List<RuleDefiniton> _supportedFormattings = new List<RuleDefiniton>
        {            
            new RuleDefiniton(@"_.*?_", RuleType.Italic, 1), // Underscores for italics
            new RuleDefiniton(@"\*.*?\*", RuleType.Bold, 1), // Stars for bold
        };

        public static IEnumerable<RuleMatch> ApplyRulesToText(string text)
        {
            var groupedByIndex = _supportedFormattings
                .SelectMany(x => FindMatches(text, x))
                .GroupBy(x => x.StartIndex)
                .OrderBy(x => x.Key);

            RuleMatch lastMatch = null;
            foreach (IGrouping<int, RuleMatch> group in groupedByIndex)
            {
                RuleMatch bestMatch = group.OrderBy(x => x.Precedence).First();
                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                {
                    continue;
                }

                yield return bestMatch;
                lastMatch = bestMatch;
            }
        }

        private static IEnumerable<RuleMatch> FindMatches(string text, RuleDefiniton matcher)
        {
            MatchCollection matches = matcher.Regex.Matches(text);
            for (int i = 0; i < matches.Count; i++)
            {
                Match currentMatch = matches[i];

                string value = null;
                if (currentMatch.Groups.Count > 2) // Have an opening and closing tag
                {
                    foreach (var group in currentMatch.Groups.OfType<object>()
                        .Skip(2) // Skip the actual match and opening tag
                        .SkipLastN(1)) // Skip the closing tag too
                    {
                        value += $"{group} "; // With trailing space for later inputs
                    }
                    value = value.Substring(0, value.Length - 1); // chop off final trailing space
                }
                else // No tags, should just be regular string.
                {
                    value = currentMatch.Value;
                }

                yield return new RuleMatch
                {
                    StartIndex = currentMatch.Index,
                    EndIndex = currentMatch.Index + currentMatch.Length,
                    RuleType = matcher.RuleType,
                    Precedence = matcher.Precedence,
                    Value = value
                };
            }
        }

        private class RuleDefiniton
        {
            public Regex Regex { get; set; }
            public RuleType RuleType { get; set; }
            public uint Precedence { get; set; }

            public RuleDefiniton(string regexString, RuleType ruleType, uint precendence)
            {
                Regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.Singleline);
                RuleType = ruleType;
                Precedence = precendence;
            }
        }
    }
}
