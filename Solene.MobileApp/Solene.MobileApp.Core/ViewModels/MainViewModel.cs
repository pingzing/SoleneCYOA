using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.ViewModels
{
    public class MainViewModel : NavigableViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly INotificationService _notificationService;

        public ObservableCollection<ProfileMoniker> CharacterList { get; } = new ObservableCollection<ProfileMoniker>();

        public MainViewModel(INavigationService navService, IProfileService profileService,
            INotificationService notificationService) : base(navService)
        {
            _profileService = profileService;
            _notificationService = notificationService;
        }

        public override async Task Activated(NavigationType navType)
        {
            var characterNames = await _profileService.GetSavedProfileNames();
            CharacterList.Clear();
            foreach(var name in characterNames)
            {
                CharacterList.Add(name);
            }

            bool success = await _notificationService.Register(Guid.Parse(CharacterList[0].Id));
        }
    }
}
