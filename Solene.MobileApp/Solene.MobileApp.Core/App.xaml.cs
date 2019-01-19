using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Solene.MobileApp.Core.Consts;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Views;
using Solene.MobileApp.Core.Views.PlayerCreation;
using System.Diagnostics;
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
            AppCenter.Start($"android={Secrets.AndroidAppCenterKey};" +
                $"uwp={Secrets.UwpAppCenterKey}", 
                typeof(Analytics), typeof(Crashes));
            if (Preferences.Get(PreferencesKeys.FirstCharacterCreationComplete, false)) //todo: any saved characters
            {
                await MainNavigationHost.NavigateToAsync(new MainPage(), false);
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
            // Handle when your app resumes
        }
    }
}
