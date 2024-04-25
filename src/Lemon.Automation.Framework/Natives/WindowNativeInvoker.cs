using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Point = System.Drawing.Point;

namespace Lemon.Automation.Framework.Natives
{
    public static class WindowNativeInvoker
    {
        public static nint WindowFromPoint(Point point)
        {
            var handle = PInvoke.WindowFromPoint(point);
            if (!handle.IsNull)
            {
                return handle.Value;
            }
            return nint.Zero;
        }

        public static Rectangle GetWindowRectangle(nint handle)
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
        public static string GetWindowText(nint handle, int buffer = 2048)
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
        public static string GetWindowClassName(nint handle, int buffer = 1024)
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
