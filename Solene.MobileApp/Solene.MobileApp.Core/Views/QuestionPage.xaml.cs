using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{

    public partial class QuestionPage : ContentPage
    {
        public QuestionPage(ChosenQuestionRequest chosen)
        {
            InitializeComponent();
            (BindingContext as QuestionPageViewModel).Parameter = chosen;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(1000);
            ApplyBindings();
        }
    }
}