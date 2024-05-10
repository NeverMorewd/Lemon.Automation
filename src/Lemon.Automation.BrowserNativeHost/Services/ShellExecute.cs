using Lemon.Automation.BrowserNativeHost.Contracts;
using Lemon.Automation.BrowserNativeHost.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class ShellExecute : IShellExecute
    {
        public int StartProcess(string processPath, IEnumerable<string> args, int timeoutInMs = -1)
        {
            if (!File.Exists(processPath))
            {
                throw new InvalidOperationException("File does not exist: " + processPath);
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo(processPath);
            

            args?.ForEach(processStartInfo.ArgumentList.Add);
            Process process = Process.Start(processStartInfo);
            if (timeoutInMs < 0)
            {
                return 0;
            }
            process?.WaitForExit(timeoutInMs);
            if (process != null && process.HasExited)
            {
                return (process?.ExitCode).Value;
            }
            process?.Kill(true);
            return -1;
        }
    }
}
