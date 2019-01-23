using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.Models;

namespace Solene.MobileApp.Core.ViewModels
{
    public class QuestionViewModel: NavigableViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly INetworkService _networkService;
        private PlayerProfile _backingProfile;

        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private Question _currentQuestion;
        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                Set(ref _currentQuestion, value);
                RaisePropertyChanged(nameof(IsNextVisible));
                RaisePropertyChanged(nameof(IsPreviousVisible));
                RaisePropertyChanged(nameof(IsFreeFormEntryEnabled));
                PreviousCommand.RaiseCanExecuteChanged();
                NextCommand.RaiseCanExecuteChanged();
                AnswerFreeFormQuestionCommand.RaiseCanExecuteChanged();
            }
        }

        // Do we have a next question?        
        public bool IsNextVisible
        {
            get
            {
                int? index = _backingProfile?.Questions?.IndexOf(CurrentQuestion);
                bool currentAnswered = CurrentQuestion?.ChosenAnswer != null;
                return index + 1 < _backingProfile?.Questions?.Count;
            }
        }

        // Do we have a previous question?        
        public bool IsPreviousVisible
        {
            get
            {
                int? index = _backingProfile?.Questions?.IndexOf(CurrentQuestion);
                return index > 0;
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Set(ref _isLoading, value);
                AnswerQuestionCommand.RaiseCanExecuteChanged();
                AnswerFreeFormQuestionCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsFreeFormEntryEnabled));
            }
        }

        public bool IsFreeFormEntryEnabled => !IsLoading && CurrentQuestion?.ChosenAnswer == null;

        public RelayCommand NextCommand { get; private set; }
        public RelayCommand PreviousCommand { get; private set; }
        public RelayCommand<string> AnswerQuestionCommand { get; private set; }
        public RelayCommand<string> AnswerFreeFormQuestionCommand { get; private set; }

        public QuestionViewModel(INavigationService navService, 
            INetworkService networkService,
            IProfileService profileService) : base(navService)
        {
            _networkService = networkService;
            _profileService = profileService;
            NextCommand = new RelayCommand(NextClicked, CanClickNext);
            PreviousCommand = new RelayCommand(PreviousClicked);
            AnswerQuestionCommand = new RelayCommand<string>(AnswerQuestionClicked, CanClickAnswers);
            AnswerFreeFormQuestionCommand = new RelayCommand<string>(AnswerQuestionClicked, CanClickFreeForm);
        }

        public override Task Activated(NavigationType navType)
        {
            var chosenQuestion = Parameter as ChosenQuestionRequest;
            _backingProfile = chosenQuestion.Profile;
            CurrentQuestion = _backingProfile.Questions[chosenQuestion.ChosenIndex];
            Title = $"Question {CurrentQuestion.SequenceNumber}";

            PreviousCommand.RaiseCanExecuteChanged();
            NextCommand.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }

        private bool CanClickNext()
        {
            if (_backingProfile == null)
            {
                return false;
            }

            // Do we have a next question?
            // If so, does our current question have an answer?
            int index = _backingProfile.Questions.IndexOf(CurrentQuestion);
            bool currentAnswered = CurrentQuestion.ChosenAnswer != null;
            return index + 1 < _backingProfile.Questions.Count
                && currentAnswered;
        }

        private void NextClicked()
        {
            int currentIndex = _backingProfile.Questions.IndexOf(CurrentQuestion);
            CurrentQuestion = _backingProfile.Questions[currentIndex + 1];
        }

        private void PreviousClicked()
        {
            int currentIndex = _backingProfile.Questions.IndexOf(CurrentQuestion);
            CurrentQuestion = _backingProfile.Questions[currentIndex - 1];
        }


        private async void AnswerQuestionClicked(string answer)
        {
            var questionAnswered = CurrentQuestion;

            // Store the CurrentQuestion, so that if the user navigates away, we retain context
            IsLoading = true;
            bool answerSuccess = await _networkService.AnswerQuestion(questionAnswered.Id, answer);
            if (!answerSuccess)
            {
                // Display error, do nothing else
                IsLoading = false;
                return;
            }

            //  - Update this question
            int index = _backingProfile.Questions.IndexOf(questionAnswered);
            _backingProfile.Questions[index].ChosenAnswer = answer;

            //  - Update and save the current profile.
            await _profileService.SaveProfile(_backingProfile);

            //  - RaiseCanExecuteChanged on the next button and the answer buttons.  
            NextCommand.RaiseCanExecuteChanged();
            AnswerQuestionCommand.RaiseCanExecuteChanged();
            AnswerFreeFormQuestionCommand.RaiseCanExecuteChanged();
            IsLoading = false;
        }

        private bool CanClickAnswers(string answer)
        {
            // Not loading, and not already answered.
            return !IsLoading && CurrentQuestion?.ChosenAnswer == null;
        }

        private bool CanClickFreeForm(string answer)
        {
            // Not loading, not already answered, and has a valid answer.
            return !IsLoading 
                && CurrentQuestion?.ChosenAnswer == null 
                && !string.IsNullOrWhiteSpace(answer);
        }
    }
}
