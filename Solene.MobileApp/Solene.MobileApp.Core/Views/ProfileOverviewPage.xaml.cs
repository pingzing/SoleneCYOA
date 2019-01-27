using System;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Solene.MobileApp.Core.Messages;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using Solene.Models;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class ProfileOverviewPage : ContentPage
    {
        private ProfileOverviewViewModel _viewmodel;
        private readonly IMessenger _messengerService;

        public ProfileOverviewPage(PlayerProfile profile)
        {
            InitializeComponent();
            _viewmodel = BindingContext as ProfileOverviewViewModel;
            _viewmodel.Parameter = profile;
            _messengerService = SimpleIoc.Default.GetInstance<IMessenger>();
            _messengerService.Register<QuestionListRefreshed>(this, ListRefreshed);
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            QuestionViewModel selected = e.Item as QuestionViewModel;
            if (selected.IsLocked)
            {
                return;
            }

            await (BindingContext as ProfileOverviewViewModel).QuestionSelected(selected);
        }

        private void ListRefreshed(QuestionListRefreshed _)
        {
            var firstUnanswered = _viewmodel.Questions.FirstOrDefault(x => x.ChosenAnswer == null);
            if (firstUnanswered == null)
            {
                // Everything has an answer, unlock everything
                foreach(var question in _viewmodel.Questions)
                {
                    question.IsLocked = false;
                }
                return;
            }

            int newUnlockIndex = _viewmodel.Questions.IndexOf(firstUnanswered);

            // Lock everything beyond the exception-index
            for (int i = newUnlockIndex; i < _viewmodel.Questions.Count; i++)
            {
                _viewmodel.Questions[i].IsLocked = true;
            }

            // ...except the one exception.
            _viewmodel.Questions[newUnlockIndex].IsLocked = false;            
        }
    }
}