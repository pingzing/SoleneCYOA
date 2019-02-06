using System.Diagnostics;

namespace Solene.SimpleParser
{
    [DebuggerDisplay("{Value}, {RuleType}")]
    public class RuleMatch
    {
        public RuleType RuleType { get; set; }
        public string Value { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public uint Precedence { get; set; }
    }
}
