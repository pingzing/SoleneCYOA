using System;

using Solene.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Solene.AdminClient.Views
{
    public sealed partial class PlayerControl : UserControl
    {
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

        private static void OnMasterMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayerControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
