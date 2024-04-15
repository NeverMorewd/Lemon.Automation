using System.Drawing;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lemon.Automation.App.UITracker.Track
{
    public class ElementHighlighter
    {
        private ElementHighlightBorder elementHighlightBorder;
        private Dispatcher _uiDispatcher;
        private readonly Thread _uiThread;
        private readonly ManualResetEventSlim _startedEvent = new(false);
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
            _uiDispatcher.InvokeAsync(() =>
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
        public void Highlight(Rectangle aRectangle)
        {
            _uiDispatcher.Invoke(new Action(() =>
            {
                elementHighlightBorder.SetCurrentElementRectBorder(aRectangle);
                elementHighlightBorder.Topmost = true;
            }));
        }
        public void Pause()
        {
            _uiDispatcher.Invoke(new Action(() =>
            {
                elementHighlightBorder.Visibility = System.Windows.Visibility.Hidden;
            }));
        }
        public void Close()
        {
            _uiDispatcher.Invoke(new Action(() =>
            {
                elementHighlightBorder.Close();
            }));
        }
    }
}
