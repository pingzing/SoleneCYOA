namespace Solene.MobileApp.Core.Messages
{
    public class LocalToastNotificationArgs
    {
        public string Text { get; set; }

        public LocalToastNotificationArgs() { }

        public LocalToastNotificationArgs(string text)
        {
            Text = text;
        }
    }
}
