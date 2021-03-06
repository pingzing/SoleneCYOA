﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Solene.MobileApp.Core.Messages;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.ViewModels
{
    public class ProfileOverviewViewModel : NavigableViewModelBase
    {
        private PlayerProfile _profile;

        private readonly INetworkService _networkService;
        private readonly IProfileService _profileService;
        private readonly INotificationService _notificationService;
        private readonly IMessenger _messengerService;

        private string _titleString;
        public string TitleString
        {
            get => _titleString;
            set => Set(ref _titleString, value);
        }

        private string _profileIdString;
        public string ProfileIdString
        {
            get => _profileIdString;
            set => Set(ref _profileIdString, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private ObservableCollection<QuestionViewModel> _questions = new ObservableCollection<QuestionViewModel>();
        public ObservableCollection<QuestionViewModel> Questions
        {
            get => _questions;
            set => Set(ref _questions, value);
        }

        public RelayCommand RefreshCommand { get; private set; }

        public RelayCommand GoToSettingsCommand { get; }

        public ProfileOverviewViewModel(INavigationService navService,
            INetworkService networkService,
            IProfileService profileService,
            INotificationService notificationService,
            IMessenger messengerService) : base(navService)
        {
            RefreshCommand = new RelayCommand(RefreshClicked);
            GoToSettingsCommand = new RelayCommand(GoToSettingsClicked);
            _networkService = networkService;
            _profileService = profileService;
            _notificationService = notificationService;
            _messengerService = messengerService;
        }

        public override async Task Activated(NavigationType navType)
        {
            if (navType == NavigationType.Forward)
            {
                _profile = (PlayerProfile)Parameter;
                TitleString = $"{_profile.PlayerInfo.Name}'s Profile";
                ProfileIdString = $"ID: {_profile.PlayerInfo.Id}";

                if (_profile.Questions != null)
                {
                    Questions = new ObservableCollection<QuestionViewModel>(
                        _profile.Questions.Select(x => new QuestionViewModel(x)));
                }
                await _notificationService.Register(_profile.PlayerInfo.Id);
            }

            await Refresh();
        }

        public async Task QuestionSelected(QuestionViewModel selected)
        {
            ChosenQuestionRequest request = new ChosenQuestionRequest
            {
                ChosenIndex = Questions.IndexOf(selected),
                Profile = _profile
            };
            await _navigationService.NavigateToViewModelAsync<QuestionPageViewModel>(request);
        }

        private async void RefreshClicked()
        {
            await Refresh();
        }

        private async Task Refresh()
        {
            IsLoading = true;
            // First update list with any changes from question-answering
            var updatedProfileResult = await _profileService.GetProfile(_profile.PlayerInfo.Id);
            if (updatedProfileResult.IsOk)
            {
                var updatedProfileQuestions = updatedProfileResult.Unwrap().Questions;
                for (int i = 0; i < Questions.Count; i++)
                {
                    Questions[i].ChosenAnswer = updatedProfileQuestions[i].ChosenAnswer;
                }
            }

            // Finally, see if the server has anything new for us
            var latestQuestionsResult = await _networkService.GetPlayerQuestions(_profile.PlayerInfo.Id);
            if (latestQuestionsResult.IsError)
            {
                // TODO: Display error. Maybe.
                IsLoading = false;
                _messengerService.Send(new QuestionListRefreshed());
                return;
            }

            var latestQuestions = latestQuestionsResult.Unwrap();
            if (latestQuestions.Count != _profile.Questions.Count
                || !_profile.Questions.SequenceEqual(latestQuestions))
            {
                _profile.Questions = latestQuestions;
                await _profileService.SaveProfile(_profile);
                Questions = new ObservableCollection<QuestionViewModel>(
                    _profile.Questions.Select(x => new QuestionViewModel(x)));
            }
            IsLoading = false;
            _messengerService.Send(new QuestionListRefreshed());
        }

        private async void GoToSettingsClicked()
        {
            await _navigationService.NavigateToViewModelAsync<SettingsViewModel>(_profile);
        }
    }
}
