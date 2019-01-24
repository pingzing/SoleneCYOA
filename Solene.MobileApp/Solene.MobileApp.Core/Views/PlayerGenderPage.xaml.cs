using Solene.MobileApp.Core.ViewModels;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class PlayerGenderPage : ContentPage
    {
        public PlayerGenderPage(string playerName)
        {
            InitializeComponent();
            (this.BindingContext as PlayerGenderViewModel).PlayerName = playerName;
        }
    }
}