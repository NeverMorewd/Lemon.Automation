using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class NativeLibraryTools
    {
        public static T GetProcAddress<T>(IntPtr moduleHandle, string functionName)
        {
            if (NativeLibrary.TryGetExport(moduleHandle, functionName, out var intPtr))
            {
                return Marshal.GetDelegateForFunctionPointer<T>(intPtr);
            }
            //Log.Error("[NativeLibraryTools] Could not get address of: " + functionName, "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Native\\NativeLibraryTools.cs", 18, "GetProcAddress");
            return default(T);
        }
    }
}
