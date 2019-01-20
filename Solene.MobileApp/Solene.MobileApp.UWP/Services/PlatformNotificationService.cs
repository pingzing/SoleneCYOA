using Solene.MobileApp.Core.Services.CrossplatInterfaces;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace Solene.MobileApp.UWP.Services
{
    public class PlatformNotificationService : IPlatformNotificationSerice
    {
        public async Task<string> GetPnsToken()
        {
            try
            {
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                return channel.Uri.ToString();
            }
            catch(Exception ex)
            {
                return null;
            }            
        }

        public string GetPushTemplate() => @"<toast><visual><binding template=""ToastGeneric""><text>$(title)</text><text>$(body)</text></binding></visual></toast>";
    }
}
