using Solene.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Solene.AdminClient.Views
{
    public sealed partial class AddQuestionControl : UserControl
    {
        public event EventHandler<Question> SubmitClicked;

        public ObservableCollection<PrefilledContainer> PrefilledAnswers { get; private set; } = new ObservableCollection<PrefilledContainer>();

        public AddQuestionControl()
        {
            this.InitializeComponent();
            PrefilledAnswers.Add(new PrefilledContainer { Answer = "" });
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var clickedElement = (sender as Button).DataContext as PrefilledContainer;
            int index = PrefilledAnswers.IndexOf(clickedElement);
            PrefilledAnswers.RemoveAt(index);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            PrefilledAnswers.Add(new PrefilledContainer { Answer = "" });
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            Question question = new Question
            {
                Title = TitleBox.Text,
                Text = BodyTextBox.Text,
                PrefilledAnswers = PrefilledAnswers.Select(x => x.Answer).ToList()
            };
            SubmitClicked?.Invoke(this, question);
        }

        public void Clear()
        {
            TitleBox.Text = "";
            BodyTextBox.Text = "";
            PrefilledAnswers.Clear();
            PrefilledAnswers.Add(new PrefilledContainer { Answer = "" });
        }
    }

    public class PrefilledContainer
    {
        public string Answer { get; set; }
    }
}
