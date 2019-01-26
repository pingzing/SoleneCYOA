using System;

namespace Solene.Models
{
    public class AdminQuestion : Question
    {
        public DateTimeOffset UpdatedTimestamp { get; set; }
    }
}
