using Solene.MobileApp.Core.Services.CrossplatInterfaces;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Services
{
    public interface INotificationService
    {
        Task<bool> Register(Guid id);
    }

    public class NotificationService : INotificationService
    {
        private readonly IPlatformNotificationSerice _platformNotificationService;
        private readonly INetworkService _networkService;

        public NotificationService(IPlatformNotificationSerice platformNotificationService,
            INetworkService networkService)
        {
            _platformNotificationService = platformNotificationService;
            _networkService = networkService;
        }

        public async Task<bool> Register(Guid id)
        {
            string pnsToken = await _platformNotificationService.GetPnsToken();
            var result = await _networkService.RegisterPushNotifications(id, new PushRegistrationRequest
            {
                PnsToken = pnsToken,
                PlatformPushTemplate = _platformNotificationService.GetPushTemplate(),
                PushPlatform = Device.RuntimePlatform == Device.UWP
                    ? PushNotificationPlatform.Windows
                    : PushNotificationPlatform.Firebase
            });

            if (result.IsError)
            {
                Debug.WriteLine($"Booo, push registration failed");
                return false;
            }

            return true;
        }
    }
}
