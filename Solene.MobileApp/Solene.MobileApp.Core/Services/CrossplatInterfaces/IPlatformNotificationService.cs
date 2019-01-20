using System.Threading.Tasks;

namespace Solene.MobileApp.Core.Services.CrossplatInterfaces
{
    public interface IPlatformNotificationSerice
    {
        Task<string> GetPnsToken();
        string GetPushTemplate();
    }
}
