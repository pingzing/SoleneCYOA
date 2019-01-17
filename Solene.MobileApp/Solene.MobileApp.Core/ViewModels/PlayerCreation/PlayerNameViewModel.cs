using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;

namespace Solene.MobileApp.Core.ViewModels.PlayerCreation
{
    public class PlayerNameViewModel : NavigableViewModelBase
    {
        private string _enteredName;        
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

        public PlayerNameViewModel(INavigationService navService) : base(navService)
        {
            NextCommand = new RelayCommand(NextClicked, CanClickNext);
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
