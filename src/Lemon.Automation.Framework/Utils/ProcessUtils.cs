using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Lemon.Automation.Framework.Utils
{
    public unsafe static class ProcessUtils
    {
        public unsafe static string? GetMainModuleFileName(IntPtr processHandle, int buffer = 1024)
        {
            try
            {
                uint bufferLength = (uint)buffer + 1;
                unsafe
                {
                    fixed (char* fileNameCharPoint = new char[buffer])
                    {
                        SafeProcessHandle safeProcess = new(processHandle, true);
                        var pwstr = new PWSTR(fileNameCharPoint);

                        BOOL ret = PInvoke.QueryFullProcessImageName(safeProcess,
                            Windows.Win32.System.Threading.PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32,
                            pwstr,
                            ref bufferLength);

                        if (ret.Value == 1)
                        {
                            return pwstr.AsSpan().ToString();
                        }
                        else
                        {
                            int errorCode = Marshal.GetLastWin32Error();
                            Console.WriteLine($"Failed to get process image name. Error code: {errorCode}");
                            throw new InvalidOperationException($"Failed to get process image name. Error code: {errorCode}");
                        }
                    }
                }
            }
            finally
            {
                PInvoke.CloseHandle(new HANDLE(processHandle));
            }
        }
    }
}
