using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IPlatformInformation
    {
        bool IsWindows { get; }

        bool IsLinux { get; }

        bool IsOSX { get; }

        bool IsDocker { get; }

        bool IsBrowser { get; }
    }
}
