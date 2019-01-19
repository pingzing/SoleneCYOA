using System;
using System.Collections.Generic;

namespace Solene.Models
{
    public class Question
    {
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<string> PrefilledAnswers { get; set; }
        public uint SequenceNumber { get; set; }
        public string ChosenAnswer { get; set; }
    }
}