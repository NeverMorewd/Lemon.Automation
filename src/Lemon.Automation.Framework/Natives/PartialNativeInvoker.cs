using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Lemon.Automation.Framework.Natives
{
    public static partial class PartialNativeInvoker
    {

        [LibraryImport("User32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetCursorPos(int X, int Y);

        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetCursorPos(ref PointWrapper lpPoint);


        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string? GetMainModuleFileName(IntPtr processHandle, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(processHandle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }

        public static void EnableWindowTransparent(IntPtr windowHandle)
        {
            var styleLong = GetWindowLong(windowHandle);
            AddWindowLong(windowHandle, styleLong, (int)SetWindowLongFlags.WS_EX_TRANSPARENT);
        }

        public static void DisableWindowTransparent(IntPtr windowHandle)
        {
            var styleLong = GetWindowLong(windowHandle);
            RemoveWindowLong(windowHandle, styleLong, (int)SetWindowLongFlags.WS_EX_TRANSPARENT);
        }

        public static void AddWindowLong(IntPtr windowHandle, int styleLong, int newLong)
        {
            PInvoke.SetWindowLong(new HWND(windowHandle), Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, 
                styleLong | newLong); 
        }

        public static void RemoveWindowLong(IntPtr windowHandle, int styleLong, int newLong)
        {
            PInvoke.SetWindowLong(new HWND(windowHandle), Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, 
                styleLong & ~newLong);
        }

        public static int GetWindowLong(IntPtr windowHandle)
        {
           return PInvoke.GetWindowLong(new HWND(windowHandle), 
               Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        }

        public static void AttachConsole()
        {
            PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
        }

        public static void FreeConsole()
        {
            PInvoke.FreeConsole();
        }
    }

    public struct PointWrapper
    {
        public int X;

        public int Y;
    }

    [Flags]
    public enum SetWindowLongFlags : uint
    {
        WS_OVERLAPPED = 0,
        WS_POPUP = 0x80000000,
        WS_CHILD = 0x40000000,
        WS_MINIMIZE = 0x20000000,
        WS_VISIBLE = 0x10000000,
        WS_DISABLED = 0x8000000,
        WS_CLIPSIBLINGS = 0x4000000,
        WS_CLIPCHILDREN = 0x2000000,
        WS_MAXIMIZE = 0x1000000,
        WS_CAPTION = 0xC00000,
        WS_BORDER = 0x800000,
        WS_DLGFRAME = 0x400000,
        WS_VSCROLL = 0x200000,
        WS_HSCROLL = 0x100000,
        WS_SYSMENU = 0x80000,
        WS_THICKFRAME = 0x40000,
        WS_GROUP = 0x20000,
        WS_TABSTOP = 0x10000,
        WS_MINIMIZEBOX = 0x20000,
        WS_MAXIMIZEBOX = 0x10000,
        WS_TILED = WS_OVERLAPPED,
        WS_ICONIC = WS_MINIMIZE,
        WS_SIZEBOX = WS_THICKFRAME,

        WS_EX_DLGMODALFRAME = 0x0001,
        WS_EX_NOPARENTNOTIFY = 0x0004,
        WS_EX_TOPMOST = 0x0008,
        WS_EX_ACCEPTFILES = 0x0010,
        WS_EX_TRANSPARENT = 0x0020,
        WS_EX_MDICHILD = 0x0040,
        WS_EX_TOOLWINDOW = 0x0080,
        WS_EX_WINDOWEDGE = 0x0100,
        WS_EX_CLIENTEDGE = 0x0200,
        WS_EX_CONTEXTHELP = 0x0400,
        WS_EX_RIGHT = 0x1000,
        WS_EX_LEFT = 0x0000,
        WS_EX_RTLREADING = 0x2000,
        WS_EX_LTRREADING = 0x0000,
        WS_EX_LEFTSCROLLBAR = 0x4000,
        WS_EX_RIGHTSCROLLBAR = 0x0000,
        WS_EX_CONTROLPARENT = 0x10000,
        WS_EX_STATICEDGE = 0x20000,
        WS_EX_APPWINDOW = 0x40000,
        WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
        WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
        WS_EX_LAYERED = 0x00080000,
        WS_EX_NOINHERITLAYOUT = 0x00100000,
        WS_EX_LAYOUTRTL = 0x00400000,
        WS_EX_COMPOSITED = 0x02000000,
        WS_EX_NOACTIVATE = 0x08000000,
    }
}
