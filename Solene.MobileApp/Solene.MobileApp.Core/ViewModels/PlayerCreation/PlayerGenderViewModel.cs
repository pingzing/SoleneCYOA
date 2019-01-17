using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.ViewModels.PlayerCreation
{
    public class PlayerGenderViewModel : NavigableViewModelBase
    {
        private readonly INetworkService _networkService;

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

        public PlayerGenderViewModel(INavigationService navService, INetworkService networkService) : base(navService)
        {
            MaleClickCommand = new RelayCommand(MaleClicked);
            FemaleClickCommand = new RelayCommand(FemaleClicked);
            CustomGenderClickCommand = new RelayCommand(CustomGenderClicked, CanClickCustomGender);
            _networkService = networkService;
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

        private async void FemaleClicked()
        {
            await CreatePlayer(new Player
            {
                Name = PlayerName,
                Gender = "Female"
            });
        }

        private async void CustomGenderClicked()
        {
            await CreatePlayer(new Player
            {
                Name = PlayerName,
                Gender = CustomPlayerGender
            });
        }

        private async Task CreatePlayer(Player player)
        {
            var createdPlayer = await _networkService.CreatePlayer(player);
            Debug.WriteLine($"Created player ID: {createdPlayer?.Id}");
        }

        private bool CanClickCustomGender()
        {
            return !string.IsNullOrWhiteSpace(CustomPlayerGender);
        }
    }
}
