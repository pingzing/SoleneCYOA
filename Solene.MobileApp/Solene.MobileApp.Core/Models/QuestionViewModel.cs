using GalaSoft.MvvmLight;
using Solene.Models;
using System;
using System.Collections.Generic;

namespace Solene.MobileApp.Core.Models
{
    public class QuestionViewModel : ViewModelBase, IEquatable<QuestionViewModel>
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<string> PrefilledAnswers { get; set; }
        public uint SequenceNumber { get; set; }

        private string _chosenAnswer;
        public string ChosenAnswer
        {
            get => _chosenAnswer;
            set
            {
                Set(ref _chosenAnswer, value);
                RaisePropertyChanged(nameof(IsSelectableInLists));
            }
        }

        private bool _isFirstUnfilledQuestion;
        public bool IsFirstUnfilledQuestion
        {
            get => _isFirstUnfilledQuestion;
            set
            {
                Set(ref _isFirstUnfilledQuestion, value);
                RaisePropertyChanged(nameof(IsSelectableInLists));
            }
        }

        public bool IsSelectableInLists => ChosenAnswer != null || IsFirstUnfilledQuestion;

        public QuestionViewModel(Question question)
        {
            Id = question.Id;
            Title = question.Title;
            Text = question.Text;
            PrefilledAnswers = question.PrefilledAnswers;
            SequenceNumber = question.SequenceNumber;
            ChosenAnswer = question.ChosenAnswer;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as QuestionViewModel);
        }

        public bool Equals(QuestionViewModel other)
        {
            return other != null &&
                   Id.Equals(other.Id) &&
                   PlayerId.Equals(other.PlayerId) &&
                   Title == other.Title &&
                   Text == other.Text &&
                   EqualityComparer<List<string>>.Default.Equals(PrefilledAnswers, other.PrefilledAnswers) &&
                   SequenceNumber == other.SequenceNumber;
        }

        public override int GetHashCode()
        {
            var hashCode = -244107798;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(PlayerId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(PrefilledAnswers);
            hashCode = hashCode * -1521134295 + SequenceNumber.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(QuestionViewModel model1, QuestionViewModel model2)
        {
            return EqualityComparer<QuestionViewModel>.Default.Equals(model1, model2);
        }

        public static bool operator !=(QuestionViewModel model1, QuestionViewModel model2)
        {
            return !(model1 == model2);
        }
    }
}
