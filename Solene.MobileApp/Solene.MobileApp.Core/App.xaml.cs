using GalaSoft.MvvmLight.Ioc;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Solene.MobileApp.Core.Consts;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.MobileApp.Core.Views;
using Solene.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Solene.MobileApp.Core
{
    public partial class App : Application
    {        
        private string _launchedBase64Question = null;
        public NavigationHost MainNavigationHost { get; private set; }
              
        public App(string launchedBase64Question = null)
        {
            // If Android's OnCreate has been called again, clear out the IoC container's registry so it's safe to re-init.
            SimpleIoc.Default.Reset();

            InitializeComponent();

            // Report binding failures ✨
            Log.Listeners.Add(new DelegateLogListener((arg1, arg2) => Debug.WriteLine(arg2)));

            // If launchedQuestion isn't null, then the app was started by tapping on
            // a toast notification that contained a question. 
            // Hold onto that, and pass it down to the ProfileService.
            _launchedBase64Question = launchedBase64Question;

            MainNavigationHost = new NavigationHost();
        }

        protected override async void OnStart()
        {
            //await MainNavigationHost.NavigateToAsync(new ProfileSelectPage(), false);
            MainPage = MainNavigationHost;

            var profileService = SimpleIoc.Default.GetInstance<IProfileService>();            
            var savedProfileNames = profileService.GetSavedProfileNames();

            // If we were launched with a question in notification, save it to the profile before we load up.
            if (!string.IsNullOrWhiteSpace(_launchedBase64Question))
            {
                string base64String = _launchedBase64Question;
                _launchedBase64Question = null;
                string questionJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
                Question launchedQuestion = JsonConvert.DeserializeObject<Question>(questionJson);
                await profileService.AddQuestionToSavedProfile(launchedQuestion);
            }

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
                    if (onlyProfile.IsError)
                    {
                        onlyProfile = await RepairProfile(savedProfileNames.First().Id);
                    }
                    await MainNavigationHost.NavigateToAsync(new ProfileOverviewPage(onlyProfile.Unwrap()), false);
                }
            }
            else
            {
                await MainNavigationHost.NavigateToAsync(new PlayerNamePage(), false);
            }
        }

        private async Task<MaybeResult<PlayerProfile, GenericErrorResult>> RepairProfile(Guid playerId)
        {
            // If we're in here, we're repairing a damaged profile
            var networkService = SimpleIoc.Default.GetInstance<INetworkService>();
            var getProfileResult = await networkService.GetPlayerProfile(playerId);
            if (getProfileResult.IsError)
            {
                await MainNavigationHost.NavigateToAsync(new ProfileSelectPage());
                await MainNavigationHost.DisplayAlert("Profile error", "Unable to load your profile. It may be corrupted.", "Okay =(");
            }

            return getProfileResult;
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
