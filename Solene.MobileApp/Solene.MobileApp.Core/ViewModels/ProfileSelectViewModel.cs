﻿using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.ViewModels
{
    public class ProfileSelectViewModel : NavigableViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly INotificationService _notificationService;

        public ObservableCollection<ProfileMoniker> CharacterList { get; } = new ObservableCollection<ProfileMoniker>();

        public ProfileSelectViewModel(INavigationService navService, IProfileService profileService) : base(navService)
        {
            _profileService = profileService;
        }

        public override Task Activated(NavigationType navType)
        {
            var characterNames = _profileService.GetSavedProfileNames();
            CharacterList.Clear();
            foreach(var name in characterNames)
            {
                CharacterList.Add(name);
            }

            return Task.CompletedTask;
        }

        public async Task SelectProfile(Guid id)
        {
            MaybeResult<PlayerProfile, GenericErrorResult> profileSelectionResult = await _profileService.GetProfile(id);
            if (profileSelectionResult.IsError)
            {
                // oh no, display error
                // also remove from list, maybe?
                return;
            }

            // Navigate to profile overview page that shows all questions
            await _navigationService.NavigateToViewModelAsync<ProfileOverviewViewModel>(profileSelectionResult.Unwrap());
        }
    }
}
