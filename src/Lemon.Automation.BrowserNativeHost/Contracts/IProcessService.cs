using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IProcessService
    {
        void IterateProcessAncestors(Process startProcess, HandleProcessCb cb);

        void IterateProcessAncestorNames(Process startProcess, HandleProcessNameCb cb);

        int? GetParentPid(int startProcessPid);
    }

    internal delegate LoopAction HandleProcessCb(Process process);
    internal delegate LoopAction HandleProcessNameCb(string name);

    internal enum LoopAction
    {
        Break,
        Continue
    }


}
