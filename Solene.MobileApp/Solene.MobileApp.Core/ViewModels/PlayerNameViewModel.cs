using System;
using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;

namespace Solene.MobileApp.Core.ViewModels
{
    public class PlayerNameViewModel : NavigableViewModelBase
    {
        private string _enteredName;
        private readonly IProfileService _profileService;

        public string EnteredName
        {
            get => _enteredName;
            set
            {
                Set(ref _enteredName, value);
                NextCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand NextCommand { get; private set; }
        public RelayCommand ImportProfileCommand { get; private set; }

        public PlayerNameViewModel(INavigationService navService, IProfileService profileService) : base(navService)
        {
            NextCommand = new RelayCommand(NextClicked, CanClickNext);
            ImportProfileCommand = new RelayCommand(ImportProfileClicked);
            _profileService = profileService;
        }

        private async void ImportProfileClicked()
        {
            await _navigationService.NavigateToViewModelAsync<ImportProfileViewModel>();
        }

        private async void NextClicked()
        {
            await _navigationService.NavigateToViewModelAsync<PlayerGenderViewModel>(EnteredName);
        }

        private bool CanClickNext()
        {
            return !string.IsNullOrWhiteSpace(EnteredName);
        }
    }
}
