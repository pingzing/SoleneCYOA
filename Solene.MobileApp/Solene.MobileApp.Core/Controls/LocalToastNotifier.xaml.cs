using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Solene.MobileApp.Core.Messages;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Controls
{
    public partial class LocalToastNotifier : ContentView
    {
        private readonly IMessenger _messenger;
        private readonly double _controlHeight;
        private readonly double _textOffset;
        private ConcurrentQueue<string> _notifications = new ConcurrentQueue<string>();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        // todo: add a queue to this thing, so that multiple notifications can be sitting, waitings
        // and an early dismiss
        public LocalToastNotifier()
        {
            InitializeComponent();
            _controlHeight = (double)Resources["ControlHeight"];
            _textOffset = (double)Resources["TextOffset"];
            _messenger = SimpleIoc.Default.GetInstance<IMessenger>();
            _messenger.Register<LocalToastNotificationArgs>(this, ShowNotificationInternal);
        }

        private async void ShowNotificationInternal(LocalToastNotificationArgs args)
        {
            await ShowNotification(args.Text);
        }

        public async Task ShowNotification(string text)
        {
            _notifications.Enqueue(text);
            while (_notifications.TryDequeue(out string dequeuedText))
            {
                await _semaphore.WaitAsync();
                await ShowOneNotification(text);
                _semaphore.Release();
            }
        }

        private async Task ShowOneNotification(string text)
        {            
            this.IsVisible = true;
            NotificationText.Text = text;
            await Task.WhenAll(
                NotificationArea.TranslateTo(0, 0, 333, Easing.CubicOut),
                NotificationText.TranslateTo(0, 0, 500, Easing.CubicOut)
            );
            await Task.Delay(2833);

            const uint endAnimMs = 500;
            var endAnimation = new Animation
            {
                { 0, 1, new Animation(t => { NotificationText.TranslationX = t; }, 0, _textOffset, Easing.CubicIn, () => NotificationText.TranslationX = -_textOffset) },
                { 0.6, 1, new Animation(a => { NotificationArea.TranslationY = a; }, 0, _controlHeight, Easing.CubicIn) }
            };
            endAnimation.Commit(this, "NotifierClose", 16, endAnimMs);
            await Task.Delay((int)endAnimMs + 32); // Plus an extra two ticks to make sure the onFinish callbacks have time to run.

            this.IsVisible = false;
        }
    }
}