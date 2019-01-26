using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Solene.AdminClient.Services;
using Solene.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Solene.AdminClient.Views
{
    public sealed partial class PlayerControl : UserControl
    {
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();

        public Player MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as Player; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static readonly DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem", typeof(Player), typeof(PlayerControl), new PropertyMetadata(null, OnMasterMenuItemPropertyChanged));

        public PlayerControl()
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
            var questions = await NetworkService.GetPlayerQuestions(MasterMenuItem.Id);
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
            var control = d as PlayerControl;
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
            Question result = await NetworkService.AddQuestion(MasterMenuItem.Id, e);
            AddQuestionFormStatusText.Text = $"Question with ID: {result?.Id} added.";
            await LoadQuestions();
        }
    }
}
