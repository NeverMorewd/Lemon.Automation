using System.Drawing;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Lemon.Native.Winx64.Natives
{
    public static class NativeGDIPlus
    {
        private static double _scalingFactor;
        public static double GetScalingFactor()
        {
            if (_scalingFactor == 0.0)
            {
                using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
                nint desktop = graphics.GetHdc();
                int logicalScreenHeight = PInvoke.GetDeviceCaps((HDC)desktop, GET_DEVICE_CAPS_INDEX.VERTRES);
                int physicalScreenHeight = PInvoke.GetDeviceCaps((HDC)desktop, GET_DEVICE_CAPS_INDEX.DESKTOPVERTRES);
                int deviceCaps = PInvoke.GetDeviceCaps((HDC)desktop, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
                graphics.ReleaseHdc(desktop);
                _scalingFactor = Math.Round(deviceCaps / 96.0, 2);
                if (_scalingFactor == 1.0)
                {
                    _scalingFactor = Math.Round(physicalScreenHeight / (double)logicalScreenHeight, 2);
                }
            }
            return _scalingFactor;
        }

        public static Point ScreenToClient(Point point, nint targetWindow)
        {
            PInvoke.ScreenToClient((HWND)targetWindow, ref point);
            return point;
        }
        public static Point ClientToScreen(Point point, nint targetWindow)
        {
            PInvoke.ScreenToClient((HWND)targetWindow, ref point);
            return point;
        }
        public static Rectangle ClientToScreen(Rectangle rect, nint targetWindow)
        {
            Point point = default;
            point.X = rect.X;
            point.Y = rect.Y;
            PInvoke.ClientToScreen((HWND)(nint)targetWindow, ref point);
            return new Rectangle(point.X, point.Y, rect.Width, rect.Height);
        }

        public static Rectangle ScreenToClient(Rectangle rect, nint targetWindow)
        {
            Point point = default;
            point.X = rect.X;
            point.Y = rect.Y;
            PInvoke.ScreenToClient((HWND)targetWindow, ref point);
            return new Rectangle(point.X, point.Y, rect.Width, rect.Height);
        }
    }
}
