using Solene.AdminClient.Models;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Solene.AdminClient.Views
{
    public sealed partial class MasterItem : UserControl, INotifyPropertyChanged
    {           
        public event EventHandler<AdminPlayerProfile> DeleteClick;        
        public event PropertyChangedEventHandler PropertyChanged;

        public AdminPlayerProfile BackingProfile => DataContext as AdminPlayerProfile;
        public DateTimeOffset LastUpdated => BackingProfile?.Questions?.LastOrDefault()?.UpdatedTimestamp ?? new DateTimeOffset();

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
            RaisePropertyChanged(nameof(BackingProfile));
            RaisePropertyChanged(nameof(LastUpdated));
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteClick?.Invoke(sender, this.DataContext as AdminPlayerProfile);
        }

        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
