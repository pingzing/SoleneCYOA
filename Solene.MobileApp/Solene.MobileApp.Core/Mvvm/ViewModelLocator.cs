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

            //Register your ViewModels here    
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PlayerNameViewModel>();
            SimpleIoc.Default.Register<PlayerGenderViewModel>();
        }

        // Page ViewModel properties, for XAML-y access
        public MainViewModel MainPage => SimpleIoc.Default.GetInstance<MainViewModel>();
        public PlayerNameViewModel PlayerNamePage => SimpleIoc.Default.GetInstance<PlayerNameViewModel>();
        public PlayerGenderViewModel PlayerGenderPage => SimpleIoc.Default.GetInstance<PlayerGenderViewModel>();

        private INavigationService InitializeNavigationService()
        {
            NavigationService navService = new NavigationService(((App)Application.Current).MainNavigationHost)
                .Configure(typeof(MainViewModel), typeof(MainPage))
                .Configure(typeof(PlayerNameViewModel), typeof(PlayerNamePage))
                .Configure(typeof(PlayerGenderViewModel), typeof(PlayerGenderPage));

            return navService;
        }
    }
}
