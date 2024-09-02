using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Lemon.Native.Winx64.Natives
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
    /// </summary>
    public static class NativeWindow
    {
        private static readonly HWND HWND_NOTOPMOST = (HWND)(-2);

        private static readonly HWND HWND_TOPMOST = (HWND)(-1);

        /// <summary>
        /// EnumWindows with default predicate
        /// </summary>
        /// <returns>window handle list</returns>
        /// <exception cref="Win32Exception"></exception>
        [Obsolete("Use EnumWindowsSafe instead.")]
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

        /// <summary>
        /// EnumWindows with custom predicate
        /// </summary>
        /// <param name="predicate">custom predicate</param>
        /// <returns>window handle list</returns>
        [Obsolete("Use EnumWindowsSafe instead.")]
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

        /// <summary>
        /// EnumWindowsSafe
        /// https://stackoverflow.com/questions/295996/is-the-order-in-which-handles-are-returned-by-enumwindows-meaningful
        /// It returns them in Z order. 
        /// First the top-most window with WS_EX_TOPMOST set, until the bottom-most window with WS_EX_TOPMOST set, then the top-most window without WS_EX_TOPMOST, though to the bottom-most window without WS_EX_TOPMOST. 
        /// Note that visibility is not a determining factor, so an invisible window that's higher in the Z-order than a visible window will still appear before it.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<nint> EnumWindowsSafe(Func<nint, bool> predicate)
        {
            EnumWindowsProcData procData = new(predicate);

            /// Alloc with default <GCHandleType cref="GCHandleType.Normal"></GCHandleType> should be manual free
            // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle?view=net-8.0
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


        /// <summary>
        /// GetRootWindow
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
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

        /// <summary>
        /// ChildWindowFromPoint
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static nint DeepestChildWindowFromPoint(nint targetWindow, Point point)
        {
            Point clientPoint = NativeGDIPlus.ScreenToClient(point, targetWindow);

            HWND childHwnd = PInvoke.ChildWindowFromPointEx(new HWND(targetWindow),
                clientPoint,
                CWP_FLAGS.CWP_SKIPINVISIBLE | CWP_FLAGS.CWP_SKIPDISABLED);

            if (childHwnd.IsNull || childHwnd.Value == nint.Zero)
            {
                childHwnd = new HWND(targetWindow);
            }
            if (!childHwnd.IsNull || childHwnd.Value == targetWindow)
            {
                return childHwnd;
            }
            return DeepestChildWindowFromPoint(childHwnd, point);
        }

        /// <summary>
        /// FindDescendant
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static nint? FindFirstDescendant(nint targetWindow, Func<nint, bool> predicate)
        {
            nint child = QueryChildWindow(targetWindow, (nint childHwnd) => predicate(childHwnd));
            if (child == nint.Zero)
            {
                return null;
            }
            return child;
        }

        /// <summary>
        /// FindAllDescendant
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<nint> FindAllDescendants(nint targetWindow, Func<nint, bool> predicate)
        {
            return from m in QueryAllChildWindows(targetWindow, (nint childHwnd) => predicate(childHwnd))
                   select m;
        }

        

        /// <summary>
        /// GetChildren
        /// what is difference between GetChildren and FindAllDescendant
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
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

        public static string GetWindowTitle(nint targetWindow) 
        {
            var textLength = PInvoke.GetWindowTextLength(new HWND(targetWindow));
            unsafe
            {
                //https://stackoverflow.com/questions/69943019/stackalloc-vs-fixed-sized-buffer-in-c-what-is-the-difference
                //char* fileNameChars = stackalloc char[textLength];

                fixed (char* fileNameChars = new char[textLength])
                {
                    var pwstr = new PWSTR(fileNameChars);
                    var ret = PInvoke.GetWindowText(new HWND(targetWindow), pwstr, textLength);

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

        public static string GetWindowClassName(nint targetWindow, int buffer = 256)
        {
            unsafe
            {
                fixed (char* fileNameCharPoint = new char[buffer])
                {
                    var pwstr = new PWSTR(fileNameCharPoint);
                    var result = PInvoke.GetClassName(new HWND(targetWindow), pwstr, buffer);

                    if (result == 0)
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

        public static int GetWindowProcessId(nint targetWindow)
        {
            unsafe
            {
                uint pid = default;
                var result = PInvoke.GetWindowThreadProcessId(new HWND(targetWindow), &pid);
                if (result == 0)
                {
                    var code = Marshal.GetLastWin32Error();
                    throw new Win32Exception(code);
                }
                return (int)pid;
            }
        }

        /// <summary>
        /// The flags member of WINDOWPLACEMENT retrieved by this function is always zero. 
        /// If the window identified by the hWnd parameter is maximized, the showCmd member is SW_SHOWMAXIMIZED. 
        /// If the window is minimized, showCmd is SW_SHOWMINIMIZED. 
        /// Otherwise, it is SW_SHOWNORMAL.
        /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowplacement
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <returns></returns>
        public static WINDOW_PLACEMENT_TYPE GetWindowPlacement(nint targetWindow)
        {
            WINDOWPLACEMENT placement = default;
            unsafe
            {
                PInvoke.GetWindowPlacement(new HWND(targetWindow), &placement);
                return PlacementTypeConvert(placement.showCmd);
            }
        }

        /// <summary>
        /// active a window
        /// </summary>
        /// <param name="targetWindow"></param>
        public static void SetForegroundWindow(nint targetWindow)
        {
            unsafe
            {
                HWND hWND = new(targetWindow);
                HWND foregroundWindow = PInvoke.GetForegroundWindow();
                uint dwMyID = PInvoke.GetCurrentThreadId();
                uint dwProcessId = default;
                uint dwCurID = PInvoke.GetWindowThreadProcessId(foregroundWindow, &dwProcessId);
                PInvoke.AttachThreadInput(dwCurID, dwMyID, true);
                PInvoke.SetWindowPos(hWND, HWND_TOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
                PInvoke.SetWindowPos(hWND, HWND_NOTOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
                PInvoke.SetForegroundWindow(hWND);
                PInvoke.AttachThreadInput(dwCurID, dwMyID, false);
                PInvoke.SetFocus(hWND);
                PInvoke.SetActiveWindow(hWND);
            }
        }

        public static nint? WindowFromPoint(Point point)
        {
            var handle = PInvoke.WindowFromPoint(point);
            if (!handle.IsNull)
            {
                return handle.Value;
            }
            return null;
        }

        public static bool IsWindowVisible(nint targetWindow)
        {
            return PInvoke.IsWindowVisible(new HWND(targetWindow));
        }

        public static void MinimizedWindowWithoutAnimation(nint targetWindow)
        {
            HWND hWnd = new(targetWindow);
            WINDOWPLACEMENT placement = default;
            PInvoke.GetWindowPlacement(hWnd, ref placement);
            WINDOWPLACEMENT lpwndpl = new()
            {
                flags = placement.flags,
                showCmd = SHOW_WINDOW_CMD.SW_MINIMIZE,
                length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
                ptMaxPosition = placement.ptMaxPosition,
                ptMinPosition = placement.ptMinPosition,
                rcNormalPosition = placement.rcNormalPosition
            };
            PInvoke.SetWindowPlacement(hWnd, in lpwndpl);
        }

        public static void ShowWindowWithoutAnimation(nint targetWindow)
        {
            HWND hWnd = new(targetWindow);
            WINDOWPLACEMENT placement = default;
            PInvoke.GetWindowPlacement(hWnd, ref placement);
            if (placement.flags == WINDOWPLACEMENT_FLAGS.WPF_RESTORETOMAXIMIZED)
            {
                WINDOWPLACEMENT lpwndpl = new()
                {
                    flags = WINDOWPLACEMENT_FLAGS.WPF_RESTORETOMAXIMIZED,
                    showCmd = SHOW_WINDOW_CMD.SW_MAXIMIZE,
                    length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
                    ptMaxPosition = placement.ptMaxPosition,
                    ptMinPosition = placement.ptMinPosition,
                    rcNormalPosition = placement.rcNormalPosition
                };
                PInvoke.SetWindowPlacement(hWnd, in lpwndpl);
            }
            else
            {
                WINDOWPLACEMENT lpwndpl = new()
                {
                    showCmd = SHOW_WINDOW_CMD.SW_SHOWNORMAL,
                    length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
                    ptMaxPosition = placement.ptMaxPosition,
                    ptMinPosition = placement.ptMinPosition,
                    rcNormalPosition = placement.rcNormalPosition
                };
                PInvoke.SetWindowPlacement(hWnd, in lpwndpl);
            }
        }





        /// <summary>
        /// GetWindowRect. Be aware of DPI
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <returns></returns>
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


        /// <summary>
        /// IsWin32Window
        /// </summary>
        /// <param name="targetHandle"></param>
        /// <returns></returns>
        public static bool IsWin32Window(nint targetHandle)
        {
            HWND parentHandle = PInvoke.GetAncestor(new HWND(targetHandle), GET_ANCESTOR_FLAGS.GA_PARENT);
            if (!((nint)parentHandle == IntPtr.Zero))
            {
                return parentHandle == PInvoke.GetDesktopWindow();
            }
            return true;
        }

        /// <summary>
        /// QueryChildWindow
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static nint QueryChildWindow(nint targetWindow, Func<nint, bool> predicate)
        {
            EnumWindowsProcData procData = new(predicate);

            /// Alloc with default <GCHandleType cref="GCHandleType.Normal"></GCHandleType> should be manual free
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

        /// <summary>
        /// QueryAllChildWindows
        /// </summary>
        /// <param name="targetWindow"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static List<nint> QueryAllChildWindows(nint targetWindow, Func<nint, bool> predicate)
        {
            EnumWindowsProcData procData = new(predicate);

            /// Alloc with default <GCHandleType cref="GCHandleType.Normal"></GCHandleType> should be manual free
            // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle?view=net-8.0
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

        /// <summary>
        /// EnumFirstProc
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static BOOL EnumFirstProc(HWND hWnd, LPARAM lParam)
        {
            /// FromIntPtr with <GCHandleType cref="GCHandleType.Weak"></GCHandleType> does not need to free
            // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle?view=net-8.0
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

        /// <summary>
        /// EnumAllProc
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static BOOL EnumAllProc(HWND hWnd, LPARAM lParam)
        {
            /// FromIntPtr with <GCHandleType cref="GCHandleType.Weak"></GCHandleType> does not need to free
            // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle?view=net-8.0
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

        private static WINDOW_PLACEMENT_TYPE PlacementTypeConvert(SHOW_WINDOW_CMD showWindowCmd)
        {
            int value = (int)showWindowCmd;
            return (WINDOW_PLACEMENT_TYPE)value;
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

    public enum WINDOW_PLACEMENT_TYPE
    {
        HIDE = 0,
        // normal
        SHOWNORMAL = 1,
        /// SW_NORMAL = 1,
        // minimized
        SHOWMINIMIZED = 2,
        // maximized
        SHOWMAXIMIZED = 3,
        /// SW_MAXIMIZE = 3,
        SHOWNOACTIVATE = 4,
        SHOW = 5,
        MINIMIZE = 6,
        SHOWMINNOACTIVE = 7,
        SHOWNA = 8,
        RESTORE = 9,
        SHOWDEFAULT = 10,
        FORCEMINIMIZE = 11,
        /// SW_MAX = 11,
    }

}
