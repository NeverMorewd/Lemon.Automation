using System.Windows;
using System.Windows.Interop;

namespace Lemon.Automation.Framework.Rx
{
    public static class RxExtensions
    {
        public static bool MakeDisposable(this Window window)
        {
            if (window != null)
            {
                window.Closing += Window_Closing;
            }
            return false;
        }

        private static void Window_Closing(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.Closing -= Window_Closing;
                if (!TellWindowIsClosed(window))
                {
                    if (window.DataContext != null && window.DataContext is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// https://stackoverflow.com/questions/381973/how-do-you-tell-if-a-wpf-window-is-closed
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        private static bool TellWindowIsClosed(Window window)
        {
            bool isClosed = false;
            if (PresentationSource.FromVisual(window) == null)
            {
                isClosed = true;
            }
            else if (!window.IsLoaded)
            {
                isClosed = true;
            }
            else
            {
                try
                {
                    WindowInteropHelper windowInteropHelper = new(window);
                    var handle = windowInteropHelper.EnsureHandle();
                    if (handle == IntPtr.Zero)
                    {
                        isClosed = true;
                    }
                }
                catch
                {
                    isClosed = true;
                }
            }
            return isClosed;
        }
    }
}
