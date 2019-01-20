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

            var app = new Core.App();
            RegisterNativeServices();
            LoadApplication(app);
        }

        private void RegisterNativeServices()
        {
            SimpleIoc.Default.Register<IPlatformNotificationSerice, PlatformNotificationService>();
        }
    }
}
