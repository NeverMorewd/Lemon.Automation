using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class ProcessService : IProcessService
    {
        private INativeService m_native;

        public ProcessService(INativeService native)
        {
            m_native = native;
        }

        public int? GetParentPid(int startProcessPid)
        {
            return m_native.GetParentProcessPid(startProcessPid);
        }

        public void IterateProcessAncestors(Process startProcess, HandleProcessCb handleParentCb)
        {
            try
            {
                if (handleParentCb == null)
                {
                    return;
                }
                HashSet<int> hashSet = new();
                Process process = startProcess;
                while (true)
                {
                    int? parentPid = GetParentPid(process.Id);
                    if (!parentPid.HasValue)
                    {
                        break;
                    }
                    int value = parentPid.Value;
                    if (hashSet.Contains(value))
                    {
                        break;
                    }
                    hashSet.Add(value);
                    Process processById = Process.GetProcessById(value);
                    if (processById == null || processById.HasExited || processById.StartTime > process.StartTime || handleParentCb(processById) == LoopAction.Break)
                    {
                        break;
                    }
                    process = processById;
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\ProcessService.cs", 77, "IterateProcessAncestors");
            }
        }

        public void IterateProcessAncestorNames(Process startProcess, HandleProcessNameCb handleCb)
        {
            IterateProcessAncestors(startProcess, (Process parentProcess) => handleCb(parentProcess.ProcessName));
        }
    }
}
