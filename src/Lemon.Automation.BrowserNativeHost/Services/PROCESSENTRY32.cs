using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal struct PROCESSENTRY32
    {
        public uint dwSize;

        public uint cntUsage;

        public uint th32ProcessID;

        public IntPtr th32DefaultHeapID;

        public uint th32ModuleID;

        public uint cntThreads;

        public uint th32ParentProcessID;

        public int pcPriClassBase;

        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }
}
