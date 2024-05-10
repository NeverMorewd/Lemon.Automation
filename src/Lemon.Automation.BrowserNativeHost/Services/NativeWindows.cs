using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class NativeWindows : INativeService
    {
        public int? GetParentProcessPid(int startProcessPid)
        {
            Kernel32 value = Kernel32.Instance.Value;
            if (value == null)
            {
                return null;
            }
            IntPtr intPtr = IntPtr.Zero;
            try
            {
                intPtr = value.CreateToolhelp32Snapshot(2u, 0u);
                PROCESSENTRY32 lppe = default(PROCESSENTRY32);
                lppe.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
                if (!value.Process32First(intPtr, ref lppe))
                {
                    //Log.Error("[GetParentPid] Process32First failed", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\NativeWindows.cs", 29, "GetParentProcessPid");
                    return null;
                }
                do
                {
                    if (startProcessPid == lppe.th32ProcessID)
                    {
                        return (int)lppe.th32ParentProcessID;
                    }
                }
                while (value.Process32Next(intPtr, ref lppe));
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\NativeWindows.cs", 43, "GetParentProcessPid");
            }
            finally
            {
                value.CloseHandle(intPtr);
            }
            return null;
        }

        public int GetCurrentSessionId()
        {
            return Process.GetCurrentProcess().SessionId;
        }
    }

}
