using Lemon.Automation.Domains;
using Lemon.Automation.Framework.Natives;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    public class Win32AutomationService
    {
        private readonly ILogger _logger;
        private readonly IElementCacheService<Win32Element> _elementCacheService;
        public Win32AutomationService(ILogger<Win32AutomationService> logger, 
            IElementCacheService<Win32Element> win32ElementCache)
        {
            _logger = logger;
            _elementCacheService = win32ElementCache;
        }
        public nint WindowFromPoint(Point point)
        {
            return WindowNativeInvoker.WindowFromPoint(point);
        }
        public Rectangle GetWindowRectangle(nint handle)
        {
            return WindowNativeInvoker.GetWindowRectangle(handle);
        }
        public string GetWindowText(nint handle, int buffer = 2048)
        {
            return WindowNativeInvoker.GetWindowText(handle, buffer);
        }
        public string GetWindowClassName(nint handle, int buffer = 1024)
        {
            return WindowNativeInvoker.GetWindowClassName(handle, buffer);
        }
    }
}
