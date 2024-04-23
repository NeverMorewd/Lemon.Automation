using Microsoft.Extensions.Logging;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Lemon.Automation.Framework.AutomationCore.Services
{
    public class Win32AutomationService
    {
        private readonly ILogger _logger;
        public Win32AutomationService(ILogger<Win32AutomationService> logger) 
        {
            _logger = logger;
        }
        public nint WindowFromPoint(Point point)
        {
            var handle = PInvoke.WindowFromPoint(point);
            if (handle == IntPtr.Zero)
            {
                _logger.LogWarning("None valid window");
            }
            return handle.Value;
        }
        public Rectangle GetWindowRectangle(nint handle)
        {
            var ret = PInvoke.GetWindowRect(new HWND(handle), out RECT lpRect);
            if (ret.Value == 0)
            {
                return default;
            }
            else
            {
                return new Rectangle(lpRect.X, lpRect.Y, lpRect.Width, lpRect.Height);
            }
        }
        public string GetWindowText(nint handle,int buffer = 2048)
        {
            int bufferLength = buffer + 1;
            unsafe
            {
                fixed (char* fileNameCharPoint = new char[buffer])
                {
                    var pwstr = new PWSTR(fileNameCharPoint);
                    var ret = PInvoke.GetWindowText(new HWND(handle), pwstr, bufferLength);

                    if (ret == 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return pwstr.AsSpan().ToString();
                    }
                }
            }
        }
        public string GetWindowClassName(nint handle, int buffer = 1024)
        {
            int bufferLength = buffer + 1;
            unsafe
            {
                fixed (char* fileNameCharPoint = new char[buffer])
                {
                    var pwstr = new PWSTR(fileNameCharPoint);
                    var ret = PInvoke.GetClassName(new HWND(handle), pwstr, bufferLength);

                    if (ret == 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return pwstr.AsSpan().ToString();
                    }
                }
            }
        }
    }
}
