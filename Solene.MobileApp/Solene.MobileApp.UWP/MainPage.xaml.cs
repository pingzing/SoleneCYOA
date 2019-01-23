using GalaSoft.MvvmLight.Ioc;
using Solene.MobileApp.Core.Services.CrossplatInterfaces;
using Solene.MobileApp.UWP.Services;

namespace Solene.MobileApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            if (App.Current.Resources.TryGetValue("launchedQuestion", out object launchedQuestion))
            {
                // if it exists, clear it after reading it
                App.Current.Resources.Remove("launchedQuestion");
            }

            var app = new Core.App((string)launchedQuestion);
            RegisterNativeServices();
            LoadApplication(app);
        }

        private void RegisterNativeServices()
        {
            SimpleIoc.Default.Register<IPlatformNotificationSerice, PlatformNotificationService>();
        }
    }
}
