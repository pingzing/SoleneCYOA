using Android.App;
using Android.Content;
using Android.Support.V4.App;
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
            string messageTitle = message.GetNotification().Title;
            string messageBody = message.GetNotification().Body;
            string base64Question = null;
            if (message?.Data?.Count > 0)
            {
                message.Data.TryGetValue("question", out base64Question);
            }

            // (Re)open the app via the main intent
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("question", base64Question);

            PendingIntent pendingIntent = PendingIntent.GetActivity(this,
                MainActivity.NotificationId,
                intent,
                PendingIntentFlags.OneShot);

            // Build and fire local notification
            var notificationBuilder = new NotificationCompat.Builder(this, PlatformNotificationService.NotificationChannelId)
                .SetSmallIcon(Resource.Mipmap.solene_flag)
                .SetContentTitle(messageTitle)
                .SetContentText(messageBody)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NotificationId, notificationBuilder.Build());

            // TODO: In addition to firing a toast notification, we should also just update
            // the underlying profile up in the cross-platform app.
        }
    }
}