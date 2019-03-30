﻿using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Solene.MobileApp.Core.Messages;
using Solene.MobileApp.Core.Models;
using Solene.MobileApp.Core.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Views
{
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel _viewModel;
        private IMessenger _messenger;

        // Flag that allows us to modify the Switch's toggle state without making it fire more network events.
        private bool _publicToggledProcessing = false;

        public SettingsPage(PlayerProfile profile)
        {
            InitializeComponent();
            _viewModel = BindingContext as SettingsViewModel;
            _viewModel.Parameter = profile;
            _messenger = SimpleIoc.Default.GetInstance<IMessenger>();
            _messenger.Register<SettingsPageCopyIdClicked>(this, CopyIdClicked);
        }

        private async void PublicSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (_publicToggledProcessing)
            {
                return;
            }
            await IsPublicToggled(e.Value);
        }

        private void PublicLabel_Tapped(object sender, System.EventArgs e)
        {
            if (_publicToggledProcessing)
            {
                return;
            }
            IsPublicSwitch.IsToggled = !IsPublicSwitch.IsToggled;
        }

        private async Task IsPublicToggled(bool newValue)
        {
            // Once we start processing the new toggle state, start ignoring any changes to the UI element,
            // in case we set it programmatically, we don't want to start new network events.
            IsPublicSwitch.IsEnabled = false;
            _publicToggledProcessing = true;            
            bool success = await _viewModel.SetProfileVisibility(newValue);
            if (!success)
            {
                IsPublicSwitch.IsToggled = false;
                await ShowNotification("Failed to set profile visibility. Unable to communicate with the server.");
            }
            _publicToggledProcessing = false;
            IsPublicSwitch.IsEnabled = true;
        }

        private async void CopyIdClicked(SettingsPageCopyIdClicked _)
        {
            await ShowNotification("Profile ID copied to clipboard!");
        }

        private async Task ShowNotification(string text)
        {
            // TODO: Turn this and its UI into a control that listens to messages broadcast by IMessenger.
            NotificationText.Text = text;
            await NotificationArea.TranslateTo(0, 0, 333, Easing.CubicOut);
            await Task.Delay(3000);
            await NotificationArea.TranslateTo(0, 50, 333, Easing.CubicIn);
        }
    }
}