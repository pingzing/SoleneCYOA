using Solene.AdminClient.Models;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Linq;
using System.Diagnostics;

namespace Solene.AdminClient.Views
{
    public sealed partial class MasterItem : UserControl
    {
        public event EventHandler<AdminPlayerProfile> DeleteClick;        

        public MasterItem()
        {
            this.InitializeComponent();                        
            DataContextChanged += MasterItem_DataContextChanged;
        }

        private void MasterItem_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var newItem = args.NewValue as AdminPlayerProfile;
            
            if (newItem == null)
            {                
                VisualStateManager.GoToState(this, "Answered", false);
                return;
            }            

            if (newItem.Questions.LastOrDefault()?.ChosenAnswer != null)
            {
                VisualStateManager.GoToState(this, "Unanswered", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Answered", false);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteClick?.Invoke(sender, this.DataContext as AdminPlayerProfile);
        }
    }
}
