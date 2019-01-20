using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using Solene.Models;
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

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Question selected = e.Item as Question;
            await (BindingContext as ProfileOverviewViewModel).QuestionSelected(selected);
        }
    }
}