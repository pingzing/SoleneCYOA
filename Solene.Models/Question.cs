using System;
using System.Collections.Generic;

namespace Solene.Models
{
    public class Question : IEquatable<Question>
    {
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<string> PrefilledAnswers { get; set; }
        public uint SequenceNumber { get; set; }
        public string ChosenAnswer { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Question);
        }

        public bool Equals(Question other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   PlayerId.Equals(other.PlayerId) &&
                   Title == other.Title &&
                   Text == other.Text &&
                   SequenceNumber == other.SequenceNumber;
        }

        public override int GetHashCode()
        {
            var hashCode = -182597323;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(PlayerId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + SequenceNumber.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Question question1, Question question2)
        {
            return EqualityComparer<Question>.Default.Equals(question1, question2);
        }

        public static bool operator !=(Question question1, Question question2)
        {
            return !(question1 == question2);
        }
    }
}