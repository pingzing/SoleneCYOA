using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Networking.PushNotifications;
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
            else if(base64Question != null)
            {
                // This happened because we clicked on a notification and the app is active
                // Use the Messenger to send it down to the either the QuestionViewModel,
                // or maybe the profile service (which would in turn fire a "hey, I updated"
                // event).
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
    }
}
