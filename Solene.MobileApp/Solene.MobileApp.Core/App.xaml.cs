using GalaSoft.MvvmLight.Ioc;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Solene.MobileApp.Core.Consts;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.MobileApp.Core.Services.CrossplatInterfaces;
using Solene.MobileApp.Core.Views;
using Solene.MobileApp.Core.Views.PlayerCreation;
using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Solene.MobileApp.Core
{
    public partial class App : Application
    {
        public NavigationHost MainNavigationHost { get; private set; }

        // Guard against resuming reinitializing on Android.
        private static bool _initialized = false;
        public App()
        {
            if (_initialized)
            {                
                return;
            }

            _initialized = true;

            // Report binding failures ✨
            Log.Listeners.Add(new DelegateLogListener((arg1, arg2) => Debug.WriteLine(arg2)));
            InitializeComponent();

            MainNavigationHost = new NavigationHost();            
            MainPage = MainNavigationHost;
        }

        protected override async void OnStart()
        {
            var profileService = SimpleIoc.Default.GetInstance<IProfileService>();
            var savedProfileNames = profileService.GetSavedProfileNames();

            AppCenter.Start($"android={Secrets.AndroidAppCenterKey};" +
                $"uwp={Secrets.UwpAppCenterKey}", 
                typeof(Analytics), typeof(Crashes));
            if (Preferences.Get(PreferencesKeys.FirstCharacterCreationComplete, false))
            {
                if (savedProfileNames.Count() > 1)
                {
                    await MainNavigationHost.NavigateToAsync(new ProfileSelectPage(), false);
                }
                else
                {
                    var onlyProfile = await profileService.GetProfile(savedProfileNames.First().Id);
                    await MainNavigationHost.NavigateToAsync(new ProfileOverviewPage(onlyProfile.Unwrap()), false);
                }
            }
            else
            {
                await MainNavigationHost.NavigateToAsync(new PlayerNamePage(), false);
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {

        }
    }
}
