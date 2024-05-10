using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    /// <summary>
    /// https://stackoverflow.com/questions/58955723/net-core-project-with-native-library-dependency
    /// </summary>
    internal class Kernel32
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        private delegate IntPtr CreateToolhelp32SnapshotFunc(uint dwFlags, uint th32ProcessID);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool Process32FirstFunc([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool Process32NextFunc([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool CloseHandleFunc([In] IntPtr hObject);

        public static Lazy<Kernel32> Instance = new Lazy<Kernel32>(delegate
        {
            if (NativeLibrary.TryLoad("kernel32.dll", out var moduleHandle))
            {
                return new Kernel32(moduleHandle);
            }
            //Log.Error("[Kernel32] Could not load kernel32.dll", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Native\\Windows\\Kernel32.cs", 32, "Instance");
            return (Kernel32)null;
        });

        public const int INVALID_HANDLE_VALUE = -1;

        private readonly IntPtr m_moduleHandle;

        private readonly CreateToolhelp32SnapshotFunc m_createToolhelp32Snapshot;

        private readonly Process32FirstFunc m_process32First;

        private readonly Process32NextFunc m_process32Next;

        private readonly CloseHandleFunc m_closeHandle;

        private Kernel32(IntPtr moduleHandle)
        {
            m_moduleHandle = moduleHandle;
            m_createToolhelp32Snapshot = NativeLibraryTools.GetProcAddress<CreateToolhelp32SnapshotFunc>(m_moduleHandle, "CreateToolhelp32Snapshot");
            m_process32First = NativeLibraryTools.GetProcAddress<Process32FirstFunc>(m_moduleHandle, "Process32First");
            m_process32Next = NativeLibraryTools.GetProcAddress<Process32NextFunc>(m_moduleHandle, "Process32Next");
            m_closeHandle = NativeLibraryTools.GetProcAddress<CloseHandleFunc>(m_moduleHandle, "CloseHandle");
        }

        public IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID)
        {
            if (m_createToolhelp32Snapshot == null)
            {
                return IntPtr.Zero;
            }
            return m_createToolhelp32Snapshot(dwFlags, th32ProcessID);
        }

        public bool CloseHandle(IntPtr handle)
        {
            if (m_closeHandle == null)
            {
                return false;
            }
            return m_closeHandle(handle);
        }

        public bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe)
        {
            if (m_process32First == null)
            {
                return false;
            }
            return m_process32First(hSnapshot, ref lppe);
        }

        public bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe)
        {
            if (m_process32Next == null)
            {
                return false;
            }
            return m_process32Next(hSnapshot, ref lppe);
        }
    }
}
