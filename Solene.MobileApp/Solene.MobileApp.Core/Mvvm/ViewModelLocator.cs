using GalaSoft.MvvmLight.Ioc;
using Solene.MobileApp.Core.Services;
using Solene.MobileApp.Core.ViewModels;
using Solene.MobileApp.Core.ViewModels.PlayerCreation;
using Solene.MobileApp.Core.Views;
using Solene.MobileApp.Core.Views.PlayerCreation;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Mvvm
{

    /*
      In the View:
      BindingContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"  
    */
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            //Register (and initialize, if necessary) your services here
            SimpleIoc.Default.Register<INavigationService>(() => InitializeNavigationService());
            SimpleIoc.Default.Register<INetworkService, NetworkService>();
            SimpleIoc.Default.Register<IProfileService, ProfileService>();
            SimpleIoc.Default.Register<INotificationService, NotificationService>();

            //Register ViewModels here    
            SimpleIoc.Default.Register<ProfileSelectViewModel>();
            SimpleIoc.Default.Register<PlayerNameViewModel>();
            SimpleIoc.Default.Register<PlayerGenderViewModel>();
            SimpleIoc.Default.Register<ProfileOverviewViewModel>();
        }

        // Page ViewModel properties, for XAML-y access
        public ProfileSelectViewModel ProfileSelect => SimpleIoc.Default.GetInstance<ProfileSelectViewModel>();
        public PlayerNameViewModel PlayerName => SimpleIoc.Default.GetInstance<PlayerNameViewModel>();
        public PlayerGenderViewModel PlayerGender => SimpleIoc.Default.GetInstance<PlayerGenderViewModel>();
        public ProfileOverviewViewModel ProfileOverview => SimpleIoc.Default.GetInstance<ProfileOverviewViewModel>();

        private INavigationService InitializeNavigationService()
        {
            NavigationService navService = new NavigationService(((App)Application.Current).MainNavigationHost)
                .Configure(typeof(ProfileSelectViewModel), typeof(ProfileSelectPage))
                .Configure(typeof(PlayerNameViewModel), typeof(PlayerNamePage))
                .Configure(typeof(PlayerGenderViewModel), typeof(PlayerGenderPage))
                .Configure(typeof(ProfileOverviewViewModel), typeof(ProfileOverviewPage));

            return navService;
        }
    }
}
