using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using System;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class ProfileSelectPage : ContentPage
    {
        public ProfileSelectPage()
        {
            InitializeComponent();
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Guid profileId = (e.Item as ProfileMoniker).Id;
            await (BindingContext as ProfileSelectViewModel).SelectProfile(profileId);
        }
    }
}
