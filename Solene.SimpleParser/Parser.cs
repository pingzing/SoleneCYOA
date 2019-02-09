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

        /// <summary>
        /// Convert the given markup text into a series of ordered, <see cref="FormattedSegment"/>s that
        /// contain the text split into segments, with a formatting rule applied exclusively to each segment.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<FormattedSegment> ArrangeText(string text)
        {
            var spans = ApplyRulesToText(text);
            if (!spans.Any())
            {
                return new[] { new FormattedSegment() { Text = text } };
            }

            return ArrangeText(spans, text);
        }

        private static IEnumerable<FormattedSegment> ArrangeText(IEnumerable<RuleMatch> spans, string originalText)
        {
            var runs = new List<FormattedSegment>();
 
            int currentIndex = 0;
            foreach (var span in spans)
            {
                //if this rule match is preceded by something that doesn't match a rule, pull it out as regular text.
                if (currentIndex < span.StartIndex)
                {
                    runs.Add(new FormattedSegment
                    {
                        Text = originalText.Substring(currentIndex, span.StartIndex - currentIndex),
                    });
                }

                runs.Add(new FormattedSegment
                {
                    Text = StripUnprintableCharacters(span.Value),
                    Formatting = span.RuleType,
                });
                currentIndex = span.EndIndex;
            }

            // Anything trailing after all the rule-matched segments needs to be pulled out too.
            int indexDiff = originalText.Length - currentIndex;
            if (indexDiff > 0)
            {
                runs.Add(new FormattedSegment
                {
                    Text = originalText.Substring(currentIndex, indexDiff)
                });
            }

            return runs;
        }

        /// <summary>
        /// Transforms text into a series of <see cref="RuleMatch"/>es, which splits the text into
        /// mutually-exclusive formatted sections, with index information about where that text section
        /// began and ended in the original text.
        /// </summary>        
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

        private static Regex _unprintables = new Regex(@"[\*_]", RegexOptions.Compiled);
        private static string StripUnprintableCharacters(string text)
        {
            return _unprintables.Replace(text, "");
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

    public class FormattedSegment
    {
        public RuleType Formatting { get; set; }
        public string Text { get; set; }
    }
}
