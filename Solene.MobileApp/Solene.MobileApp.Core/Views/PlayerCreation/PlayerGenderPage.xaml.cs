using Solene.MobileApp.Core.ViewModels.PlayerCreation;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views.PlayerCreation
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