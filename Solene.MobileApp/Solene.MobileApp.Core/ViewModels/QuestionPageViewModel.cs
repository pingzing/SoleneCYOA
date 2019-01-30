using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Solene.MobileApp.Core.Messages;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.ViewModels
{
    public class QuestionPageViewModel : NavigableViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly INetworkService _networkService;
        private readonly IMessenger _messengerService;
        private PlayerProfile _backingProfile;

        public string Title => $"Question {CurrentQuestion?.SequenceNumber}";

        private Question _currentQuestion;
        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                Set(ref _currentQuestion, value);
                QuestionChanged();
            }
        }

        private string _freeformText;
        public string FreeformText
        {
            get => _freeformText;
            set => Set(ref _freeformText, value);
        }

        public string ChosenAnswer => CurrentQuestion?.ChosenAnswer;

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

        private Guid _testGuid = Guid.Parse("ea47e33b-34e8-4454-9db2-a797d481e339");
        public bool IsTestAnswerVisible
        {
            get
            {
                if (Guid.TryParse(_backingProfile?.PlayerInfo.Name, out Guid parsedNameGuid))
                {
                    return !IsNextVisible && parsedNameGuid == _testGuid;
                }
                return false;
            }
        }

        public bool IsFreeFormEntryEnabled => !IsLoading && ChosenAnswer == null;

        public RelayCommand NextCommand { get; private set; }
        public RelayCommand PreviousCommand { get; private set; }
        public RelayCommand<string> AnswerQuestionCommand { get; private set; }
        public RelayCommand<string> AnswerFreeFormQuestionCommand { get; private set; }
        public RelayCommand TestAnswerCommand { get; private set; }

        public QuestionPageViewModel(INavigationService navService,
            INetworkService networkService,
            IProfileService profileService,
            IMessenger messengerService) : base(navService)
        {
            _networkService = networkService;
            _profileService = profileService;
            NextCommand = new RelayCommand(NextClicked, CanClickNext);
            PreviousCommand = new RelayCommand(PreviousClicked);
            AnswerQuestionCommand = new RelayCommand<string>(AnswerQuestionClicked, CanClickAnswers);
            AnswerFreeFormQuestionCommand = new RelayCommand<string>(AnswerQuestionClicked, CanClickFreeForm);
            TestAnswerCommand = new RelayCommand(TestAnswerClicked);
            _messengerService = messengerService;
            _messengerService.Register<ProfileUpdated>(this, OnProfileUpdated);
        }

        private void OnProfileUpdated(ProfileUpdated updatedProfileMessage)
        {
            if (_backingProfile.PlayerInfo.Id == updatedProfileMessage.NewQuestion.PlayerId
                && !_backingProfile.Questions.Any(x => x.Id == updatedProfileMessage.NewQuestion.Id))
            {
                _backingProfile.Questions.Add(updatedProfileMessage.NewQuestion);
                QuestionChanged();
            }
        }

        public override Task Activated(NavigationType navType)
        {
            var chosenQuestion = Parameter as ChosenQuestionRequest;
            _backingProfile = chosenQuestion.Profile;
            CurrentQuestion = _backingProfile.Questions[chosenQuestion.ChosenIndex];

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
            bool currentAnswered = ChosenAnswer != null;
            return index + 1 < _backingProfile.Questions.Count
                && currentAnswered;
        }

        private void NextClicked()
        {
            int currentIndex = _backingProfile.Questions.IndexOf(CurrentQuestion);
            if (currentIndex + 1 >= _backingProfile.Questions.Count)
            {
                return;
            }
            CurrentQuestion = _backingProfile.Questions[currentIndex + 1];
        }

        private void PreviousClicked()
        {
            int currentIndex = _backingProfile.Questions.IndexOf(CurrentQuestion);
            if (currentIndex == 0)
            {
                return;
            }
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
            RaisePropertyChanged(nameof(ChosenAnswer));

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
            return !IsLoading && ChosenAnswer == null;
        }

        private bool CanClickFreeForm(string answer)
        {
            // Not loading, not already answered, and has a valid answer.
            return !IsLoading
                && ChosenAnswer == null
                && !string.IsNullOrWhiteSpace(answer);
        }

        private void QuestionChanged()
        {
            FreeformText = "";
            RaisePropertyChanged(nameof(IsNextVisible));
            RaisePropertyChanged(nameof(IsPreviousVisible));
            RaisePropertyChanged(nameof(IsFreeFormEntryEnabled));
            RaisePropertyChanged(nameof(ChosenAnswer));
            RaisePropertyChanged(nameof(Title));
            RaisePropertyChanged(nameof(IsTestAnswerVisible));
            PreviousCommand.RaiseCanExecuteChanged();
            NextCommand.RaiseCanExecuteChanged();
            AnswerFreeFormQuestionCommand.RaiseCanExecuteChanged();
        }

        private bool _gettingTestAnswer = false;
        private async void TestAnswerClicked()
        {
            if (_gettingTestAnswer)
            {
                return;
            }

            _gettingTestAnswer = true;

            bool response = await _networkService.SimulateDeveloperAnswer(_backingProfile.PlayerInfo.Id);
            if (!response)
            {
                _gettingTestAnswer = false;
                return;
            }

            var latestQuestionsResult = await _networkService.GetPlayerQuestions(_backingProfile.PlayerInfo.Id);
            if (latestQuestionsResult.IsError)
            {
                _gettingTestAnswer = false;
                return;
            }

            var latestQuestions = latestQuestionsResult.Unwrap();
            var lastQuestion = latestQuestions.LastOrDefault();
            if (lastQuestion == null)
            {
                _gettingTestAnswer = false;
                return;
            }
            
            if (_backingProfile.Questions.Any(x => x.Id == lastQuestion.Id))
            {
                _gettingTestAnswer = false;
                return;
            }

            _backingProfile.Questions.Add(lastQuestion);
            QuestionChanged();
            _gettingTestAnswer = false;
        }
    }
}
