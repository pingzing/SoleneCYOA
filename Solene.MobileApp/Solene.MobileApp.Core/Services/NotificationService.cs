using Solene.MobileApp.Core.Services.CrossplatInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.Services
{
    public interface INotificationService
    {

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

        public async Task<bool> Register()
        {
            string pnsToken = await _platformNotificationService.GetPnsToken();
        }
    }
}
