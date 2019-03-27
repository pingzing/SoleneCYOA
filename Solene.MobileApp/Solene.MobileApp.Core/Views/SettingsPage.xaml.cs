using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel _viewModel;

        public SettingsPage(PlayerProfile profile)
        {
            InitializeComponent();
            _viewModel = BindingContext as SettingsViewModel;
            _viewModel.Parameter = profile;
        }
    }
}