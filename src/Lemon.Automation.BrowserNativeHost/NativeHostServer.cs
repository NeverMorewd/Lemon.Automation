using Lemon.Automation.BrowserNativeHost.Contracts;
using Lemon.Automation.BrowserNativeHost.Services;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class NativeHostServer : IDisposable
    {
        private BrowserExtensionConnector m_extension;

        private Thread m_nativeHostServerThread;

        private CancellationTokenSource m_cancelTokenSource = new();

        private bool m_isDisposed = false;

        public NativeHostServer(BrowserExtensionConnector extensionConnector)
        {
            m_extension = extensionConnector;
            StartNativeHostThread();
        }

        public void Dispose()
        {
            if (!m_isDisposed)
            {
                m_isDisposed = true;
                m_cancelTokenSource.Cancel();
                m_nativeHostServerThread.Join();
                m_cancelTokenSource.Dispose();
            }
        }

        private void StartNativeHostThread()
        {
            m_nativeHostServerThread = new Thread(NativeHostServerPipeThread);
            m_nativeHostServerThread.Name = "NativeHostServerPipeThread";
            m_nativeHostServerThread.Start();
        }

        private void NativeHostServerPipeThread()
        {
            //Log.Info("Started", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 60, "NativeHostServerPipeThread");
            try
            {
                CancellationToken token = m_cancelTokenSource.Token;
                IBrowserContext browserContext = Service.Get<IBrowserContext>();
                IBrowserBridgePipeService browserBridgePipeService = Service.Get<IBrowserBridgePipeService>();
                BrowserName browserContextOfThisProcess = browserContext.GetBrowserContextOfThisProcess();
                using NamedPipeServerStream namedPipeServerStream = browserBridgePipeService.CreateServerPipe(browserContextOfThisProcess);
                if (namedPipeServerStream == null)
                {
                    //Log.Error($"Could not start server pipe for browserName: {browserContextOfThisProcess}", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 70, "NativeHostServerPipeThread");
                    return;
                }
                PipeProtocol pipeProtocol = new PipeProtocol(namedPipeServerStream);
                while (true)
                {
                    namedPipeServerStream.WaitForConnectionAsync().Wait(token);
                    ProcessNativeHostRequest(pipeProtocol);
                    try
                    {
                        namedPipeServerStream.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 91, "NativeHostServerPipeThread");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //Log.Info("Pipe read canceled", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 97, "NativeHostServerPipeThread");
            }
            catch (Exception ex3)
            {
                //Log.Error(ex3.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 101, "NativeHostServerPipeThread");
            }
            //Log.Info("Finished", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 103, "NativeHostServerPipeThread");
        }

        private void ProcessNativeHostRequest(PipeProtocol pipeProtocol)
        {
            try
            {
                ReadOnlySequence<byte> readOnlySequence = pipeProtocol.Read();
                if (readOnlySequence.IsEmpty)
                {
                    //Log.Error("Client disconnected", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 114, "ProcessNativeHostRequest");
                    return;
                }
                NativeHostRequestHeader nativeHostRequestHeader = JsonSerializer.Deserialize(readOnlySequence.FirstSpan, NativeHostRequestHeader_SourceGenerationContext.Default.NativeHostRequestHeader);
                ResponseFromExtension responseFromExtension = m_extension.SendRequest(pipeProtocol.Read(), nativeHostRequestHeader.TimeoutMs);
                NativeHostResponseHeader nativeHostResponseHeader = new()
                {
                    ResponseCode = responseFromExtension.ResponseCode
                };
                pipeProtocol.Write(JsonSerializer.SerializeToUtf8Bytes(nativeHostResponseHeader, NativeHostResponseHeader_SourceGenerationContext.Default.NativeHostResponseHeader));
                pipeProtocol.Write(responseFromExtension.ResponseData);
            }
            catch (Exception ex)
            {
                //Log.Error("ex: " + ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\NativeHostServer.cs", 131, "ProcessNativeHostRequest");
            }
        }
    }
}
