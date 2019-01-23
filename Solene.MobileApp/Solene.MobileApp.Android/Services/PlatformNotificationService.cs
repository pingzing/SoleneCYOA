using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Firebase.Iid;
using Solene.MobileApp.Core.Services.CrossplatInterfaces;

namespace Solene.MobileApp.Droid.Services
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT"})]
    public class PlatformNotificationService : FirebaseInstanceIdService, IPlatformNotificationSerice
    {
        private const string TAG = "SoleneFirebaseIIDService";
        private const string NotificationChannel = "solene_android_notification_channel";

        private Context _appContext = Application.Context;        
        private TaskCompletionSource<string> _getFirebaseTokenTask = new TaskCompletionSource<string>();

        public async Task<string> GetPnsToken()
        {
            int playAvailable = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_appContext);
            if (playAvailable != ConnectionResult.Success)
            {
                return null;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(NotificationChannel, "FCM Notifications", NotificationImportance.Default)
                {
                    Description = "Firebase cloud message appear in this channel."
                };

                try
                {
                    var notificationManager = _appContext.GetSystemService(NotificationService) as NotificationManager;
                    notificationManager?.CreateNotificationChannel(channel);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }            

            string existingId = FirebaseInstanceId.Instance.Token;
            if (existingId != null)
            {
                return existingId;
            }
            else
            {
                return await _getFirebaseTokenTask.Task;
            }
        }

        public string GetPushTemplate()
        {
            return @"{  
    ""notification"": {
        ""title"" : ""$(title)"",
        ""body"": ""$(body)"",
        ""data"": {
            ""question"": ""$(question)""
        }
    }    
}";
        }

        public override void OnTokenRefresh()
        {
            var refreshedtoken = FirebaseInstanceId.Instance.Token;
            _getFirebaseTokenTask.SetResult(refreshedtoken);
        }        
    }
}