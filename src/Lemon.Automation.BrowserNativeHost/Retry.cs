using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class Retry
    {
        public static async Task<bool> WithTimeout(int totalTimeoutMs, int sleepTimeMs, RetryLoopActionCb actionCb)
        {
            if (actionCb == null)
            {
                return false;
            }
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.CancelAfter(totalTimeoutMs);
            CancellationToken ct = cancelTokenSource.Token;
            do
            {
                try
                {
                    if (await actionCb(ct) == LoopAction.Break)
                    {
                        return true;
                    }
                }
                catch (Exception ex2)
                {
                    Exception ex = ex2;
                    //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Retry.cs", 34, "WithTimeout");
                }
                await Task.Delay(sleepTimeMs);
            }
            while (!ct.IsCancellationRequested);
            return false;
        }
    }

    internal delegate Task<LoopAction> RetryLoopActionCb(CancellationToken ct);
}
