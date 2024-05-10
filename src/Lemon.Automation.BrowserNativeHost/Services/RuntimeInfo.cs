using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Lemon.Automation.BrowserNativeHost.Contracts;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class RuntimeInfo : IRuntimeInfo
    {
        public bool IsOSPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }

        public bool IsWasm()
        {
            return RuntimeInformation.OSArchitecture == Architecture.Wasm || RuntimeInformation.ProcessArchitecture == Architecture.Wasm;
        }
    }
}
