using GalaSoft.MvvmLight.Command;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.Models;
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

        private string _titleString;
        public string TitleString
        {
            get => _titleString;
            set => Set(ref _titleString, value);
        }

        private ObservableCollection<Question> _questions = new ObservableCollection<Question>();
        public ObservableCollection<Question> Questions
        {
            get => _questions;
            set => Set(ref _questions, value);
        }

        public RelayCommand RefreshCommand { get; private set; }

        public ProfileOverviewViewModel(INavigationService navService,
            INetworkService networkService,
            IProfileService profileService,
            INotificationService notificationService) : base(navService)
        {
            RefreshCommand = new RelayCommand(RefreshClicked);
            _networkService = networkService;
            _profileService = profileService;
            _notificationService = notificationService;
        }

        public override async Task Activated(NavigationType navType)
        {
            if (navType == NavigationType.Forward)
            {
                _profile = (PlayerProfile)Parameter;
                TitleString = $"{_profile.PlayerInfo.Name}'s Profile";

                Questions = new ObservableCollection<Question>(_profile.Questions);
                await _notificationService.Register(_profile.PlayerInfo.Id);
            }

            await Refresh();
        }

        private async void RefreshClicked()
        {
            await Refresh();
        }

        private async Task Refresh()
        {            
            var latestQuestionsResult = await _networkService.GetPlayerQuestions(_profile.PlayerInfo.Id);
            if (latestQuestionsResult.IsError)
            {
                // TODO: Display error. Maybe.
                return;
            }

            var latestQuestions = latestQuestionsResult.Unwrap();
            if (latestQuestions.Count != _profile.Questions.Count
                || !_profile.Questions.SequenceEqual(latestQuestions))
            {
                _profile.Questions = latestQuestions;
                await _profileService.SaveProfile(_profile);
                Questions = new ObservableCollection<Question>(_profile.Questions);
            }
        }
    }
}
