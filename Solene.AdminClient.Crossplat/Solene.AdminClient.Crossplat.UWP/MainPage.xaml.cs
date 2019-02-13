using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace Solene.AdminClient.Crossplat.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadApplication(new Solene.AdminClient.Crossplat.App());
        }
    }
}
