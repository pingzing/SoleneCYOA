﻿using System;
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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //if (Intent.Extras != null)
            //{
            //    foreach (var key in Intent.Extras.KeySet())
            //    {
            //        var value = Intent.Extras.GetString(key);
            //        System.Diagnostics.Debug.WriteLine($"Intent Key: {key}, Val: {value}");
            //    }
            //}

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            var app = new Core.App();
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