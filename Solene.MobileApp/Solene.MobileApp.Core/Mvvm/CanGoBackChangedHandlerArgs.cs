namespace Solene.MobileApp.Core.Mvvm
{
    public class CanGoBackChangedHandlerArgs
    {
        public bool NewCanGoBack { get; private set; }

        public CanGoBackChangedHandlerArgs(bool newCanGoBack)
        {
            NewCanGoBack = newCanGoBack;
        }
    }
}
