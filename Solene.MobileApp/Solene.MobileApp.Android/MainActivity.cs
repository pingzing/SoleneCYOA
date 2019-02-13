using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using GalaSoft.MvvmLight.Ioc;
using Solene.MobileApp.Core.Services.CrossplatInterfaces;
using Solene.MobileApp.Droid.Services;
using Xamarin.Forms;

namespace Solene.MobileApp.Droid
{
    [Activity(Label = "Solene",
        Icon = "@mipmap/solene_flag",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]    
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private const string TAG = "MainActivity";
        internal const int NotificationId = 100;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            string base64Question = null;
            if (Intent.Extras != null)
            {
                string questionKey = "question";
                if (Intent.Extras.ContainsKey(questionKey))
                {
                    base64Question = Intent.Extras.GetString(questionKey);
                }
            }

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            var app = new Core.App(base64Question);
            RegisterNativeServices();
            LoadApplication(app);
        }

        private void RegisterNativeServices()
        {
            SimpleIoc.Default.Register<IPlatformNotificationSerice, PlatformNotificationService>();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}