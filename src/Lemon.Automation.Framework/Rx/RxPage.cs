using System.Windows.Controls;

namespace Lemon.Automation.Framework.Rx
{
    public abstract class RxPage : Page, IDisposable
    {
        public RxPage()
        {
            Unloaded += RxPage_Unloaded;
        }

        private void RxPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Unloaded -= RxPage_Unloaded;
            Dispose();
        }

        public virtual void Dispose()
        {
            if (DataContext != null && DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
