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
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            return channel.Uri.ToString();
        }
    }
}
