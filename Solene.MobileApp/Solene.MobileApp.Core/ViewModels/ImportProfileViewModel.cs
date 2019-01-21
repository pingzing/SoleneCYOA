using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Consts;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Solene.MobileApp.Core.ViewModels
{
    public class ImportProfileViewModel : NavigableViewModelBase
    {
        private readonly INetworkService _networkService;
        private readonly IProfileService _profileService;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Set(ref _isLoading, value);
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        private string _invalidIdErrorText;
        public string InvalidIdErrorText
        {
            get => _invalidIdErrorText;
            set
            {
                Set(ref _invalidIdErrorText, value);
                RaisePropertyChanged(nameof(IsInvalidIdErrorVisible));
            }
        }
        public bool IsInvalidIdErrorVisible => !string.IsNullOrEmpty(InvalidIdErrorText);

        private string _networkErrorText;
        public string NetworkErrorText
        {
            get => _networkErrorText;
            set
            {
                Set(ref _networkErrorText, value);
                RaisePropertyChanged(nameof(IsNetworkErrorVisible));
            }
        }
        public bool IsNetworkErrorVisible => !string.IsNullOrEmpty(NetworkErrorText);

        public RelayCommand<string> SubmitCommand { get; private set; }

        public ImportProfileViewModel(INavigationService navService,
            INetworkService networkService,
            IProfileService profileService) : base(navService)
        {
            _networkService = networkService;
            _profileService = profileService;
            SubmitCommand = new RelayCommand<string>(SubmitClicked, CanClickSubmit);
        }

        public override Task Activated(NavigationType navType)
        {
            InvalidIdErrorText = null;
            NetworkErrorText = null;
            IsLoading = false;
            return Task.CompletedTask;
        }

        private bool CanClickSubmit(string playerId)
        {
            if (!Guid.TryParse(playerId, out Guid guidPlayerId))
            {
                InvalidIdErrorText = "Entered ID is not a valid player ID.";
                return false;
            }

            InvalidIdErrorText = null;
            return !IsLoading && !string.IsNullOrWhiteSpace(playerId);
        }

        private async void SubmitClicked(string playerId)
        {
            NetworkErrorText = null;
            IsLoading = true;

            // Possible TODO: Check to see if we already know about this player locally
            Guid playerGuid = Guid.Parse(playerId);
            var playerResult = await _networkService.GetPlayer(playerGuid);
            if (playerResult.IsError)
            {
                NetworkErrorText = $"Unable to retrieve a player with that ID. The server said: {playerResult.UnwrapError()}";
                IsLoading = false;
                return;
            }

            Player player = playerResult.Unwrap();
            var questions = await _networkService.GetPlayerQuestions(player.Id);
            if (questions.IsError)
            {
                // Not fatal, just annoying. These can be fetched later.
                Debug.WriteLine($"Unable to fetch player questions for {player.Name}: {player.Id}");
            }

            PlayerProfile profile = new PlayerProfile
            {
                PlayerInfo = player,
                Questions = questions.UnwrapOr(null)
            };

            await _profileService.SaveProfile(profile);
            Preferences.Set(PreferencesKeys.FirstCharacterCreationComplete, true);

            int profileCount = _profileService.GetSavedProfileNames().Count();
            if (profileCount > 1)
            {
                await _navigationService.NavigateToViewModelAsync<ProfileSelectViewModel>();

            }
            else
            {
                await _navigationService.NavigateToViewModelAsync<ProfileOverviewViewModel>(profile);
            }

            _navigationService.ClearBackStack();
            IsLoading = false;
        }
    }
}
