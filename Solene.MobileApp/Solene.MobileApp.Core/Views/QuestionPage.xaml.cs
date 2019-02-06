using GalaSoft.MvvmLight.Ioc;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.Services;
using Solene.MobileApp.Core.ViewModels;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class QuestionPage : ContentPage
    {
        private readonly IParsingService _parsingService;

        public QuestionPage(ChosenQuestionRequest chosen)
        {
            InitializeComponent();
            (BindingContext as QuestionPageViewModel).Parameter = chosen;
            _parsingService = SimpleIoc.Default.GetInstance<IParsingService>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(1000);
            ApplyBindings();
        }
    }
}