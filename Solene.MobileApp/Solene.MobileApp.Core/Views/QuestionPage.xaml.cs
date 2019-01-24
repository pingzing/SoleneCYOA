using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
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
    }
}