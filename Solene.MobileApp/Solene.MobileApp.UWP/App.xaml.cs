using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using Solene.MobileApp.Core.Services;
using Solene.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Solene.MobileApp.UWP
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {            
            await OnLaunchedOrActivated(e);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await OnLaunchedOrActivated(args);
        }

        private async Task OnLaunchedOrActivated(IActivatedEventArgs args)
        {
            string base64Question = null;
            if (args.Kind == ActivationKind.ToastNotification)
            {
                var activatedArgs = args as ToastNotificationActivatedEventArgs;
                base64Question = activatedArgs.Argument;
                Resources["launchedQuestion"] = base64Question;
            }

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                Xamarin.Forms.Forms.Init(args);

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                object mainPageArgs = (args as LaunchActivatedEventArgs)?.Arguments;
                rootFrame.Navigate(typeof(MainPage), mainPageArgs);
            }
            else if(!string.IsNullOrWhiteSpace(base64Question))
            {
                // This happened because we clicked on a notification and the app is active
                // Update the active profile, which will trigger the Messenger to fire an
                // update message.
                var profileService = SimpleIoc.Default.GetInstance<IProfileService>();
                if (profileService != null)
                {
                    string questionJson = DecodeQuestionString(base64Question);
                    Question launchedQuestion = JsonConvert.DeserializeObject<Question>(questionJson);
                    await profileService.AddQuestionToSavedProfile(launchedQuestion);
                }
            }

            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
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
    }
}
