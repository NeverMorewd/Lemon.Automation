using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IShellExecute
    {
        int StartProcess(string processPath, IEnumerable<string> args, int timeoutInMs = -1);
    }
}
