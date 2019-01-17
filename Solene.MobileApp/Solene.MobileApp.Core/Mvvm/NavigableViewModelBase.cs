using GalaSoft.MvvmLight;
using Solene.MobileApp.Core.Services;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.Mvvm
{
    public class NavigableViewModelBase : ViewModelBase, INavigable
    {
        protected INavigationService _navigationService;

        /// <summary>
        /// An all-purpose container for passing in navigation parameters from the View upon initialization.
        /// </summary>
        public object Parameter { get; set; }

        public NavigableViewModelBase(INavigationService navService)
        {
            _navigationService = navService;
        }

        /// <summary>
        /// Fires when the ViewModel is navigated to via the NavigationService.
        /// If you don't want to mark your overrides as async, return a 
        /// CompletedTask using the <see cref="Task.CompletedTask"/>.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Activated(NavigationType navType)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Fires when the ViewModel is navigated from via the NavigationService.        
        /// If you don't want to mark your overrides as async, return a 
        /// CompletedTask using the <see cref="Task.CompletedTask"/>.
        /// </summary>
        public virtual async Task Deactivated()
        {
            await Task.CompletedTask;
        }
    }
}
