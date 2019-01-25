using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace Solene.MobileApp.Droid.Services
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT"})]
    public class SoleneFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "SoleneFirebaseMessagingService";

        public override void OnMessageReceived(RemoteMessage message)
        {
            string base64Question = null;
            if (message?.Data?.Count > 0)
            {
                message.Data.TryGetValue("question", out base64Question);
            }

            // TODO: Display local notification using the notificationbuilder and notificationmanager
        }
    }
}