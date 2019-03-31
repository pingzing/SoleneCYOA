using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Solene.MobileApp.Core.Messages;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Solene.MobileApp.Core.ViewModels
{
    public class SettingsViewModel : NavigableViewModelBase
    {
        private readonly INetworkService _networkService;
        private readonly IProfileService _profileService;
        private readonly IMessenger _messenger;

        private PlayerProfile _profile;
        private PlayerProfile Profile
        {
            get => _profile;
            set
            {
                _profile = value;
                RaisePropertyChanged(nameof(ProfileName));
                RaisePropertyChanged(nameof(ProfileIdString));
                RaisePropertyChanged(nameof(ProfileGender));
                RaisePropertyChanged(nameof(IsProfilePublic));
            }
        }

        public string ProfileName => _profile?.PlayerInfo?.Name;
        public string ProfileIdString => $"ID: {_profile?.PlayerInfo?.Id}";
        public string ProfileGender => _profile?.PlayerInfo?.Gender;
        public bool IsProfilePublic => _profile?.PlayerInfo?.IsPublic ?? false;

        public RelayCommand CopyIdCommand { get; private set; }
        public RelayCommand NewProfileCommand { get; private set; }
        public RelayCommand ImportProfileCommand { get; private set; }        

        public SettingsViewModel(INavigationService navService,
            INetworkService networkService,
            IProfileService profileService,
            IMessenger messenger) : base(navService)
        {
            _networkService = networkService;
            _profileService = profileService;
            _messenger = messenger;

            CopyIdCommand = new RelayCommand(CopyIdClicked);
            NewProfileCommand = new RelayCommand(NewProfileClicked);
            ImportProfileCommand = new RelayCommand(ImportProfileClicked);            
        }

        public override Task Activated(NavigationType navType)
        {
            if (navType == NavigationType.Forward)
            {
                Profile = (PlayerProfile)Parameter;
            }

            return Task.CompletedTask;
        }

        private async void CopyIdClicked()
        {
            await Clipboard.SetTextAsync(_profile.PlayerInfo.Id.ToString("N"));
            _messenger.Send(new LocalToastNotificationArgs { Text = "Profile ID copied to clipboard." });
        }

        private async void NewProfileClicked()
        {
            await _navigationService.NavigateToViewModelAsync<PlayerNameViewModel>();
        }

        private async void ImportProfileClicked()
        {
            await _navigationService.NavigateToViewModelAsync<ImportProfileViewModel>();
        }

        public async Task<bool> SetProfileVisibility(bool newIsPublic)
        {
            return await _networkService.SetProfileVisibility(_profile.PlayerInfo.Id, newIsPublic);
        }
    }
}
