using System.Threading.Tasks;

namespace Solene.MobileApp.Core.Mvvm
{
    public interface INavigable
    {
        Task Activated(NavigationType navType);

        Task Deactivated();
    }
}
