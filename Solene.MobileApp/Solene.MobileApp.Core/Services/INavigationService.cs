using Solene.MobileApp.Core.Mvvm;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Services
{
    
    public interface INavigationService
    {        
        Task GoBackAsync(bool animated);
        Task NavigateToViewModelAsync<T>(bool animated = true) where T : INavigable;        
        Task NavigateToViewModelAsync(Type vmType, bool animated = true);
        Task NavigateToViewModelAsync(Type vmType, object parameter, bool animated = true);
        Task NavigateToPageAsync<T>(bool animated = true) where T : Page;
        Task NavigateToViewModelAsync<T>(object parameter, bool animated = true) where T : INavigable;
        Task NavigateToPageAsync<T>(object parameter, bool animated = true) where T : Page;       

        void ClearBackStack();

        bool CanGoBack { get; }

        event EventHandler<CanGoBackChangedHandlerArgs> CanGoBackChanged;
    }
}
