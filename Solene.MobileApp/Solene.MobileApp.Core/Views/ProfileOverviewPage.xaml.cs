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
        private int _currentSpecialUnlockedIndex = -1;

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
                _currentSpecialUnlockedIndex = -1;
                // Everything has an answer, unlock everything
                foreach(var question in _viewmodel.Questions)
                {
                    question.IsLocked = false;
                }
                return;
            }

            int newUnlockIndex = _viewmodel.Questions.IndexOf(firstUnanswered);
            if (newUnlockIndex == _currentSpecialUnlockedIndex
                && _viewmodel.Questions[newUnlockIndex].IsLocked == false)
            {
                // If it's still unlocked from a previous unlock, we don't have to do anything
                return;
            }

            _currentSpecialUnlockedIndex = newUnlockIndex;

            // Lock everything beyond the exception-index
            for (int i =_currentSpecialUnlockedIndex; i < _viewmodel.Questions.Count; i++)
            {
                _viewmodel.Questions[i].IsLocked = true;
            }

            // ...except the one exception.
            _viewmodel.Questions[newUnlockIndex].IsLocked = false;            
        }
    }
}