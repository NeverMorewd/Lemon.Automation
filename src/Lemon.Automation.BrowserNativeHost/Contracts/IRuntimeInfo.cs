using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IRuntimeInfo
    {
        bool IsOSPlatform(OSPlatform platform);

        bool IsWasm();
    }
}
