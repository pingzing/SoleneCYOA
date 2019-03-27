using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.ViewModels
{
    public class SettingsViewModel : NavigableViewModelBase
    {
        private PlayerProfile _profile;

        public SettingsViewModel(INavigationService navService) : base(navService)
        {

        }

        public override Task Activated(NavigationType navType)
        {
            if (navType == NavigationType.Forward)
            {
                _profile = (PlayerProfile)Parameter;
            }

            return Task.CompletedTask;
        }
    }
}
