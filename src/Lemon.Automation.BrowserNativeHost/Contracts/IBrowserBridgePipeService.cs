using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IBrowserBridgePipeService
    {
        NamedPipeServerStream CreateServerPipe(BrowserName browserName);

        PipeStream GetConnectedClientPipe(BrowserName browserName, int connectTimeoutMs);

        Task<PipeStream> GetConnectedClientPipeAsync(BrowserName browserName, int connectTimeoutMs);

        bool IsAnyServerPipeRunning(BrowserName browserName);

        Task<bool> WaitServerPipeAsync(BrowserName browserName, int connectTimeoutMs);

        bool WaitServerPipe(BrowserName browserName, int connectTimeoutMs);
    }
}
