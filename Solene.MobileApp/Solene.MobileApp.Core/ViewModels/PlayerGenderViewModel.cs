using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Consts;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.MobileApp.Core.Views;
using Solene.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Solene.MobileApp.Core.ViewModels
{
    public class PlayerGenderViewModel : NavigableViewModelBase
    {
        private readonly INetworkService _networkService;
        private readonly IProfileService _profileService;
        private string _playerName;
        public string PlayerName
        {
            get => _playerName;
            set => Set(ref _playerName, value);
        }

        private string _customPlayerGender;
        public string CustomPlayerGender
        {
            get => _customPlayerGender;
            set
            {
                Set(ref _customPlayerGender, value);
                CustomGenderClickCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                Set(ref _isLoading, value);
                MaleClickCommand.RaiseCanExecuteChanged();
                FemaleClickCommand.RaiseCanExecuteChanged();
                CustomGenderClickCommand.RaiseCanExecuteChanged();
            }
        }

        public PlayerGenderViewModel(INavigationService navService,
            INetworkService networkService,
            IProfileService profileService)
            : base(navService)
        {
            MaleClickCommand = new RelayCommand(MaleClicked, CanClickMale);
            FemaleClickCommand = new RelayCommand(FemaleClicked, CanClickFemale);
            CustomGenderClickCommand = new RelayCommand(CustomGenderClicked, CanClickCustomGender);
            _networkService = networkService;
            _profileService = profileService;
        }

        public RelayCommand MaleClickCommand { get; private set; }
        public RelayCommand FemaleClickCommand { get; private set; }
        public RelayCommand CustomGenderClickCommand { get; private set; }

        private async void MaleClicked()
        {
            await CreatePlayer(new Player
            {
                Name = PlayerName,
                Gender = "Male"
            });
        }

        private bool CanClickMale()
        {
            return !IsLoading;
        }

        private async void FemaleClicked()
        {
            await CreatePlayer(new Player
            {
                Name = PlayerName,
                Gender = "Female"
            });
        }

        private bool CanClickFemale()
        {
            return !IsLoading;
        }

        private async void CustomGenderClicked()
        {
            await CreatePlayer(new Player
            {
                Name = PlayerName,
                Gender = CustomPlayerGender
            });
        }


        private bool CanClickCustomGender()
        {
            return !string.IsNullOrWhiteSpace(CustomPlayerGender) && !IsLoading;
        }

        private async Task CreatePlayer(Player player)
        {
            IsLoading = true;
            var playerResult = await _networkService.CreatePlayer(player);

            if (playerResult.IsError)
            {
                // todo: show error message      
                IsLoading = false;
                return;
            }

            var createdPlayer = playerResult.Unwrap();
            var questions = await _networkService.GetPlayerQuestions(createdPlayer.Id);
            if (questions.IsError)
            {
                // todo: this isn't actually fatal, just annoying. Just have to
                // make sure we get the inital questions later
                Debug.WriteLine($"Unable to fetch player questions for {player.Name}: {player.Id}");
            }
            IsLoading = false;

            PlayerProfile profile = new PlayerProfile
            {
                PlayerInfo = createdPlayer,
                Questions = questions.UnwrapOr(null),
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
                _navigationService.ClearBackStack();
                await _navigationService.NavigateToViewModelAsync<QuestionPageViewModel>(new ChosenQuestionRequest
                {
                    ChosenIndex = 0,
                    Profile = profile
                });
            }            
        }
    }
}
