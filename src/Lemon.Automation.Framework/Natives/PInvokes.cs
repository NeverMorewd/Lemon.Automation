using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;

namespace Lemon.Automation.Framework.Natives
{
    public static partial class PInvokes
    {

        [LibraryImport("User32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetCursorPos(int X, int Y);

        [LibraryImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetCursorPos(ref Point lpPoint);
    }

    public struct Point
    {
        public int X;

        public int Y;
    }
}
