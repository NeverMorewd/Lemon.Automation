using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IBrowserContext
    {
        BrowserName GetBrowserContextOfThisProcess();
    }
}
