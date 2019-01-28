using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Solene.AdminClient.Models;
using Solene.AdminClient.Services;
using Solene.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Solene.AdminClient.Views
{
    public sealed partial class PlayerProfilePage : Page
    {
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();

        public AdminPlayerProfile MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as AdminPlayerProfile; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static readonly DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem", typeof(Player), typeof(PlayerProfilePage), new PropertyMetadata(null, OnMasterMenuItemPropertyChanged));

        public PlayerProfilePage()
        {
            InitializeComponent();
        }

        private async Task LoadQuestions()
        {
            if (MasterMenuItem == null)
            {
                return;
            }
            Questions.Clear();
            AddQuestionFormStatusText.Text = "";
            var questions = await NetworkService.GetPlayerQuestions(MasterMenuItem.PlayerInfo.Id);
            if (questions != null)
            {
                foreach(var question in questions)
                {
                    Questions.Add(question);
                }
            }           
        }

        private static async void OnMasterMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayerProfilePage;
            await control.LoadQuestions();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadQuestions();
        }

        private async void AddQuestionControl_SubmitClicked(object sender, Question e)
        {
            AddQuestionFormStatusText.Text = "";
            AddQuestionForm.Clear();
            Question result = await NetworkService.AddQuestion(MasterMenuItem.PlayerInfo.Id, e);
            AddQuestionFormStatusText.Text = $"Question with ID: {result?.Id} added.";
            await LoadQuestions();
            QuestionsList.ScrollIntoView(Questions.Last(), ScrollIntoViewAlignment.Leading);
        }
    }
}
