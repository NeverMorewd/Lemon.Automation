using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Dwm;
using System;

namespace Lemon.Native.Winx64.Natives
{
    public static class NativeWindow
    {
        public static IEnumerable<nint> EnumWindowsDefault()
        {
            nint found = IntPtr.Zero;
            List<nint> windows = [];

            try
            {
                var result = PInvoke.EnumWindows(WindowFilter, new LPARAM((nint)PInvoke.GetDesktopWindow()));
                if (result.Value != 1)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new Win32Exception($"{nameof(PInvoke.EnumWindows)}:{code}");
                }
            }
            catch (Exception ex)
            {
                var code = Marshal.GetLastWin32Error();
                throw new Win32Exception(code, ex.Message);
            }

            BOOL WindowFilter(
                HWND hWnd,
                LPARAM lparam)
            {
                if (!PInvoke.IsWindowVisible(hWnd))
                {
                    return true;
                }

                int exStyle = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
                if (((uint)exStyle & 0x80u) != 0)
                {
                    return true;
                }

                if (((uint)exStyle & 0x8000000u) != 0)
                {
                    return true;
                }

                if ((nint)PInvoke.GetWindow(hWnd, GET_WINDOW_CMD.GW_OWNER) != IntPtr.Zero)
                {
                    return true;
                }

                windows.Add((nint)hWnd);
                return true;
            }

            return windows;
        }

        public static IEnumerable<nint> EnumWindows(Func<nint, bool> predicate)
        {
            nint found = IntPtr.Zero;
            List<nint> windows = [];

            PInvoke.EnumWindows(
                (hWnd, lParam) =>
                {
                    if (predicate(hWnd))
                    {
                        windows.Add((nint)hWnd);
                    }

                    return true;
                }, new LPARAM(IntPtr.Zero));

            return windows;
        }

        public static IEnumerable<nint> EnumWindowsSafe(Func<nint, bool> predicate)
        {
            EnumWindowsProcData procData = new(predicate);
            GCHandle procDataHandle = GCHandle.Alloc(procData);
            nint procDataPtr = GCHandle.ToIntPtr(procDataHandle);

            try
            {
                var result = PInvoke.EnumWindows(EnumAllProc, procDataPtr);
                if (result != true)
                {
                    return [];
                }           
            }
            finally
            {
                procDataHandle.Free();
            }

            return procData.WindowList;
        }

        public static nint GetRootWindow(nint targetWindow)
        {
            HWND rootHwnd = PInvoke.GetAncestor(new HWND(targetWindow), GET_ANCESTOR_FLAGS.GA_ROOT);
            if (rootHwnd.IsNull)
            {
                throw new Win32Exception("GetAncestor return null!");
            }
            return rootHwnd.Value;
        }

        /// <summary>
        /// GetOwner
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <returns></returns>
        public static nint? GetOwner(nint targetWindow)
        {
            HWND desktopHwnd = PInvoke.GetDesktopWindow();
            HWND ownerHwnd = PInvoke.GetWindow((HWND)(nint)targetWindow, GET_WINDOW_CMD.GW_OWNER);
            if ((nint)ownerHwnd == IntPtr.Zero || desktopHwnd == ownerHwnd)
            {
                return null;
            }
            return ownerHwnd.Value;
        }

        /// <summary>
        /// GetRootOwner
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public static nint GetRootOwner(nint targetWindow)
        {
            var rootOwner = GetOwner(targetWindow);
            if (!rootOwner.HasValue)
            {
               var result = PInvoke.GetAncestor((HWND)targetWindow, GET_ANCESTOR_FLAGS.GA_ROOTOWNER);
                if (result.IsNull) 
                {
                    throw new Win32Exception("GetAncestor return null!");
                }
                return result.Value;
            }
            while (rootOwner.HasValue)
            {
                var nextRootOwner = GetOwner(rootOwner.Value);
                if (!nextRootOwner.HasValue)
                {
                    return rootOwner.Value;
                }
                rootOwner = nextRootOwner;
            }
            return rootOwner.Value;
        }

        public static bool IsWin32Window(nint targetHandle)
        {
            HWND parentHandle = PInvoke.GetAncestor(new HWND(targetHandle), GET_ANCESTOR_FLAGS.GA_PARENT);
            if (!((nint)parentHandle == IntPtr.Zero))
            {
                return parentHandle == PInvoke.GetDesktopWindow();
            }
            return true;
        }

        public unsafe static Rectangle GetWindowRect(nint targetWindow)
        {
            RECT rect = default;
            if ((int)PInvoke.DwmGetWindowAttribute(new HWND(targetWindow), DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, &rect, (uint)Marshal.SizeOf(rect)) == 0)
            {
                var factor = NativeGDIPlus.GetScalingFactor();

                return new Rectangle((int)(rect.left / factor), 
                    (int)(rect.top / factor), 
                    (int)(rect.Width / factor), 
                    (int)(rect.Height / factor));
            }
            if ((bool)PInvoke.GetWindowRect(new HWND(targetWindow), out var rct))
            {
                return rct;
            }
            return Rectangle.Empty;
        }

        public static nint Win32HandleFromPoint(nint targetWindow, Point point)
        {
            Point clientPoint = NativeGDIPlus.ScreenToClient(point, targetWindow);
            HWND childHwnd = PInvoke.ChildWindowFromPointEx(new HWND(targetWindow), clientPoint, CWP_FLAGS.CWP_SKIPINVISIBLE | CWP_FLAGS.CWP_SKIPDISABLED);
            if (childHwnd.IsNull || childHwnd.Value == nint.Zero)
            {
                childHwnd = new HWND(targetWindow);
            }
            if (!childHwnd.IsNull || childHwnd.Value == targetWindow)
            {
                return childHwnd;
            }
            return Win32HandleFromPoint(childHwnd, point);
        }

        public static nint? FindDescendant(nint targetWindow, Func<nint, bool> predicate)
        {
            nint child = QueryChildWindow(targetWindow, (nint childHwnd) => predicate(childHwnd));
            if (child == nint.Zero)
            {
                return null;
            }
            return child;
        }

        public static nint[] FindAllDescendant(nint targetWindow, Func<nint, bool> predicate)
        {
            return (from m in QueryAllChildWindows(targetWindow, (nint childHwnd) => predicate(childHwnd))
                    select m).ToArray();
        }
        public static IntPtr QueryChildWindow(nint targetWindow, Func<nint, bool> predicate)
        {
            EnumWindowsProcData procData = new(predicate);

            // Alloc with default cref = "GCHandleType.Normal" should be manual free
            // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle?view=net-8.0
            GCHandle procDataHandle = GCHandle.Alloc(procData);
            nint procDataPtr = GCHandle.ToIntPtr(procDataHandle);
            try
            {
                PInvoke.EnumChildWindows(new HWND(targetWindow), EnumFirstProc, procDataPtr);
            }
            finally
            {
                procDataHandle.Free();
            }
            return procData.WindowList.FirstOrDefault();
        }
        public static List<nint> QueryAllChildWindows(nint targetWindow, Func<nint, bool> predicate)
        {
            EnumWindowsProcData procData = new(predicate);
            GCHandle procDataHandle = GCHandle.Alloc(procData);
            nint procDataPtr = GCHandle.ToIntPtr(procDataHandle);
            try
            {
                PInvoke.EnumChildWindows(new HWND(targetWindow), EnumAllProc, procDataPtr);
            }
            finally
            {
                procDataHandle.Free();
            }
            return procData.WindowList;
        }
        public static IEnumerable<nint> GetChildren(nint targetWindow, Func<nint, bool> predicate)
        {
            HWND childHwnd = PInvoke.GetWindow(new HWND(targetWindow), GET_WINDOW_CMD.GW_CHILD);
            while (!childHwnd.IsNull && childHwnd.Value != nint.Zero)
            {
                nint childWindow = childHwnd.Value;
                if (predicate(childWindow))
                {
                    yield return childWindow;
                }
                childHwnd = PInvoke.GetWindow(childHwnd, GET_WINDOW_CMD.GW_HWNDNEXT);
            }
        }

        private static BOOL EnumFirstProc(HWND hWnd, LPARAM lParam)
        {
            GCHandle procDataHandle = GCHandle.FromIntPtr((nint)lParam);
            if (procDataHandle.Target == null)
            {
                return false;
            }

            if (procDataHandle.Target is EnumWindowsProcData procData)
            {
                bool num = procData.Predicate((nint)hWnd);
                if (num)
                {
                    procData.WindowList.Add((nint)hWnd);
                }
                return !num;
            }
            return false;
        }

        private static BOOL EnumAllProc(HWND hWnd, LPARAM lParam)
        {
            GCHandle procDataHandle = GCHandle.FromIntPtr((nint)lParam);
            if (procDataHandle.Target == null)
            {
                return false;
            }
            if (procDataHandle.Target is EnumWindowsProcData procData && procData.Predicate((nint)hWnd))
            {
                procData.WindowList.Add((nint)hWnd);
            }
            return true;
        }

        /// <summary>
        /// for enum windows
        /// </summary>
        /// <param name="predicate"></param>
        private readonly struct EnumWindowsProcData(Func<nint, bool> predicate)
        {
            public List<nint> WindowList
            {
                get;
            } = [];

            public Func<nint, bool> Predicate
            {
                get;
            } = predicate;
        }
    }

}
