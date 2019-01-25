using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Service.Notification;

namespace Solene.MobileApp.Droid.Services
{
    [Service] //permission: android.permission.BIND_NOTIFICATION_LISTENER_SERVICE
    [IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
    public class SoleneNotificationListener : NotificationListenerService
    {
        public override void OnNotificationPosted(StatusBarNotification notification)
        {

        }
    }
}