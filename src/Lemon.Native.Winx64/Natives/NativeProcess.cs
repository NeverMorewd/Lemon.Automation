using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;

namespace Lemon.Native.Winx64.Natives
{
    public static class NativeProcess
    {
        public static string GetMainModuleFilePath(nint processHandle, uint buffer = 1024)
        {
            unsafe
            {
                fixed (char* filePathCharPoint = new char[buffer])
                {
                    var pwstr = new Windows.Win32.Foundation.PWSTR(filePathCharPoint);
                    var result = PInvoke.QueryFullProcessImageName(new SafeProcessHandle(processHandle, true), Windows.Win32.System.Threading.PROCESS_NAME_FORMAT.PROCESS_NAME_WIN32, pwstr, ref buffer);
                    
                    if(result.Value != 1)
                    {
                        Marshal.GetLastPInvokeError();
                        var message = Marshal.GetLastPInvokeErrorMessage();
                        var code = Marshal.GetLastWin32Error();
                        throw new Win32Exception(code);
                    }
                    
                    var filePath = pwstr.AsSpan().ToString();
                    return filePath;
                }
            }
        }

    }
}
