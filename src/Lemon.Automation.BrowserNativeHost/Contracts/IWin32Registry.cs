using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IWin32Registry
    {
        int? ReadIntValue(Win32RegistryHive hive, string keyPath, string valueName);
    }

    internal enum Win32RegistryHive
    {
        CurrentUser,
        LocalMachine
    }
}
