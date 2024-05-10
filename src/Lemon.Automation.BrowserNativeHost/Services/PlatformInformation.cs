using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class PlatformInformation : IPlatformInformation
    {
        public bool IsBrowser { get; }

        public bool IsWindows { get; }

        public bool IsLinux { get; }

        public bool IsOSX { get; }

        public bool IsDocker { get; }

        public PlatformInformation(IEnvironmentProvider environmentProvider, IRuntimeInfo runtimeInfo)
        {
            bool flag = IsDockerEnvironment(environmentProvider);
            IsWindows = runtimeInfo.IsOSPlatform(OSPlatform.Windows);
            IsLinux = runtimeInfo.IsOSPlatform(OSPlatform.Linux) && !flag;
            IsOSX = runtimeInfo.IsOSPlatform(OSPlatform.OSX) && !flag;
            IsDocker = runtimeInfo.IsOSPlatform(OSPlatform.Linux) && flag;
            IsBrowser = runtimeInfo.IsWasm();
        }

        private bool IsDockerEnvironment(IEnvironmentProvider environmentProvider)
        {
            string environmentVariable = environmentProvider.GetEnvironmentVariable("UIPATH_DOCKER_PLATFORM");
            if (string.IsNullOrWhiteSpace(environmentVariable))
            {
                return false;
            }
            bool flag;
            return bool.TryParse(environmentVariable, out flag) && flag;
        }
    }
}
