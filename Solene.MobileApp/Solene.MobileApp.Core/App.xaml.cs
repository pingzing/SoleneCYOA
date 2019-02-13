using GalaSoft.MvvmLight.Ioc;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Solene.MobileApp.Core.Consts;
using Solene.MobileApp.Core.Converters;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Mvvm;
using Solene.MobileApp.Core.Services;
using Solene.MobileApp.Core.Views;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new QuestionJsonConverter() }
            };
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
            AppCenter.Start($"android={Secrets.AndroidAppCenterKey};" +
                $"uwp={Secrets.UwpAppCenterKey}",
                typeof(Analytics), typeof(Crashes));

            var profileService = SimpleIoc.Default.GetInstance<IProfileService>();

            await MainNavigationHost.NavigateToAsync(new Page(), false);
            MainPage = MainNavigationHost;            
            var savedProfileNames = profileService.GetSavedProfileNames();

            // If we were launched with a question in notification, save it to the profile before we load up.
            Question launchedQuestion = null;
            if (!string.IsNullOrWhiteSpace(_launchedBase64Question))
            {
                string base64String = _launchedBase64Question;
                _launchedBase64Question = null;
                string questionJson = DecodeQuestionString(base64String);
                launchedQuestion = JsonConvert.DeserializeObject<Question>(questionJson);
                await profileService.AddQuestionToSavedProfile(launchedQuestion);
            }
            
            if (!(Preferences.Get(PreferencesKeys.FirstCharacterCreationComplete, false)))
            {
                await MainNavigationHost.NavigateToAsync(new PlayerNamePage(), false);
                MainNavigationHost.ClearBackStack();
                return;
            }
            
            if (savedProfileNames.Count() > 1)
            {
                await MainNavigationHost.NavigateToAsync(new ProfileSelectPage(), false);
                MainNavigationHost.ClearBackStack();

                if (launchedQuestion != null)
                {
                    var profile = await NavigateToProfile(launchedQuestion.PlayerId, profileService);
                    await NavigateToLaunchedQuestion(launchedQuestion, profile);
                }
            }            
            else
            {
                var profile = await NavigateToProfile(savedProfileNames.First().Id, profileService);
                MainNavigationHost.ClearBackStack();

                if (launchedQuestion != null)
                {
                    await NavigateToLaunchedQuestion(launchedQuestion, profile);
                }
            }            
        }

        private async Task<PlayerProfile> NavigateToProfile(Guid profileId, IProfileService profileService)
        {
            var onlyProfile = await profileService.GetProfile(profileId);
            if (onlyProfile.IsError)
            {
                onlyProfile = await RepairProfile(profileId);
            }
            var playerProfile = onlyProfile.Unwrap();
            await MainNavigationHost.NavigateToAsync(new ProfileOverviewPage(playerProfile), false);
            return playerProfile;
        }

        private async Task NavigateToLaunchedQuestion(Question launchedQuestion, PlayerProfile profile)
        {
            // We should navigate to either the question in the toast notification,
            // OR, the latest unanswered question (in the unlikely scenario the user still has
            // an unanswered question and has just received a new question)
            int targetQuestionIndex = profile.Questions.FindLastIndex(x => x.ChosenAnswer == null);
            await MainNavigationHost.NavigateToAsync(new QuestionPage(new ChosenQuestionRequest
            {
                ChosenIndex = targetQuestionIndex,
                Profile = profile
            }));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {

        }

        private string DecodeQuestionString(string base64String)
        {
            byte[] decompressedBytes;

            using (var inputStream = new MemoryStream(Convert.FromBase64String(base64String)))            
            using (var decompressorStream = new GZipStream(inputStream, CompressionMode.Decompress))                
            using (var outStream = new MemoryStream())
            {
                decompressorStream.CopyTo(outStream);
                decompressedBytes = outStream.ToArray();
            }

            return Encoding.UTF8.GetString(decompressedBytes);
        }

        // TODO: Move this to somewhere that gets executed anytime a profile gets opened.
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
    }
}
