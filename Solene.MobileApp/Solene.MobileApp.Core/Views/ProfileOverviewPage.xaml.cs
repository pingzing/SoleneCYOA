using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class ProfileOverviewPage : ContentPage
    {
        public ProfileOverviewPage(PlayerProfile profile)
        {
            InitializeComponent();
            (BindingContext as ProfileOverviewViewModel).Parameter = profile;
        }
    }
}