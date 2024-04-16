using System.Drawing;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lemon.Automation.App.UITracker.Track
{
    public class ElementHighlighter
    {
        private ElementHighlightBorder? elementHighlightBorder;
        private Dispatcher? _uiDispatcher;
        private readonly Thread _uiThread;
        private readonly ManualResetEventSlim _startedEvent = new(false);
        private bool _enable = true;
        public ElementHighlighter()
        {
            _uiThread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                _uiDispatcher = Dispatcher.CurrentDispatcher;
                _startedEvent.Set();
                Dispatcher.Run();
            });
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.IsBackground = true;
            _uiThread.Start();
            _startedEvent.Wait();
            Install();
        }
        public void Highlight(Rectangle aRectangle)
        {
            if (!_enable)
            {
                return;
            }
            _uiDispatcher?.Invoke(new Action(() =>
            {
                if (elementHighlightBorder != null)
                {
                    elementHighlightBorder.SetCurrentElementRectBorder(aRectangle);
                    elementHighlightBorder.Topmost = true;
                }
            }));
        }
        public void Enable()
        {
            _enable = true;
            _uiDispatcher?.Invoke(new Action(() =>
            {
                if (elementHighlightBorder != null)
                {
                    elementHighlightBorder.Visibility = System.Windows.Visibility.Visible;
                }
            }));
        }
        public void Disable()
        {
            _enable = false;
            _uiDispatcher?.Invoke(new Action(() =>
            {
                if (elementHighlightBorder != null)
                {
                    elementHighlightBorder.Visibility = System.Windows.Visibility.Collapsed;
                }
            }));
        }
        public void Uninstall()
        {
            _uiDispatcher?.Invoke(new Action(() =>
            {
                elementHighlightBorder?.Close();
                elementHighlightBorder = null;
            }));
        }
        public void Install()
        {
            _uiDispatcher?.InvokeAsync(() =>
            {
                elementHighlightBorder = new ElementHighlightBorder(new Rectangle(1, 1, 1, 1),
                    new SolidColorBrush(Colors.Red),
                    true,
                    true,
                    0)
                {
                    Visibility = System.Windows.Visibility.Visible
                };
                elementHighlightBorder.Show();
            });
        }
    }
}
