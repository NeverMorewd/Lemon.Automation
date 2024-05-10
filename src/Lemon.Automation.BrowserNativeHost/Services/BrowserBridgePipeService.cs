using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class BrowserBridgePipeService : IBrowserBridgePipeService
    {
        private const string LocalMachine = ".";

        private const string DefaultPipeNameBase = "UiPath.BrowserBridge.Portable.Pipe";

        private const int DefaultClientConnectSleepTimeMs = 100;

        public const string PipeRootWindows = "\\\\.\\pipe\\";

        public const string PipeRootUnix = "/tmp/";

        private static PipeDirection s_pipeDirection = PipeDirection.InOut;

        private static int s_maxNumberOfServerInstances = 1;

        private static PipeTransmissionMode transmissionMode = PipeTransmissionMode.Byte;

        private static PipeOptions s_pipeOptions = PipeOptions.Asynchronous;

        public string PipeNameBase { get; private init; }

        public int ClientConnectSleepTimeMs { get; private init; }

        private IPlatformInformation PlatformInfo { get; init; }

        private INativeService NativeService { get; init; }

        public BrowserBridgePipeService(IPlatformInformation platformInfo, INativeService nativeService)
            : this(platformInfo, nativeService, "UiPath.BrowserBridge.Portable.Pipe", 100)
        {
        }

        public BrowserBridgePipeService(IPlatformInformation platformInfo, INativeService nativeService, string pipeNameBase, int clientConnectSleepTimeMs)
        {
            PipeNameBase = pipeNameBase;
            ClientConnectSleepTimeMs = clientConnectSleepTimeMs;
            PlatformInfo = platformInfo;
            NativeService = nativeService;
        }

        public NamedPipeServerStream CreateServerPipe(BrowserName browserName)
        {
            Process currentProcess = Process.GetCurrentProcess();
            int currentSessionId = NativeService.GetCurrentSessionId();
            return CreateServerPipe(browserName, currentSessionId, currentProcess.Id);
        }

        public NamedPipeServerStream CreateServerPipe(BrowserName browserName, int serverSessionId, int serverPid)
        {
            try
            {
                string serverPipeName = GetServerPipeName(PipeNameBase, browserName, serverSessionId, serverPid, PlatformInfo.IsWindows);
                //Log.Verbose("Creating server pipeName: " + serverPipeName, "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 65, "CreateServerPipe");
                return new NamedPipeServerStream(serverPipeName, s_pipeDirection, s_maxNumberOfServerInstances, transmissionMode, s_pipeOptions);
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 70, "CreateServerPipe");
            }
            return null;
        }

        public PipeStream GetConnectedClientPipe(BrowserName browserName, int connectTimeoutMs)
        {
            return Task.Run(async () => await GetConnectedClientPipeAsync(browserName, connectTimeoutMs)).Result;
        }

        public async Task<PipeStream> GetConnectedClientPipeAsync(BrowserName browserName, int connectTimeoutMs)
        {
            NamedPipeClientStream out_connectedClientPipe = null;
            await Retry.WithTimeout(connectTimeoutMs, ClientConnectSleepTimeMs, async delegate
            {
                string serverPipeName = FindServerPipe(PipeNameBase, browserName, PlatformInfo.IsWindows);
                if (string.IsNullOrEmpty(serverPipeName))
                {
                    //Log.Warning($"No server found for browserName: {browserName}", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 97, "GetConnectedClientPipeAsync");
                    return LoopAction.Break;
                }
                NamedPipeClientStream clientPipe = null;
                bool hasConnected = false;
                try
                {
                    clientPipe = new NamedPipeClientStream(".", serverPipeName, s_pipeDirection, s_pipeOptions);
                    await clientPipe.ConnectAsync(ClientConnectSleepTimeMs);
                    out_connectedClientPipe = clientPipe;
                    hasConnected = true;
                    return LoopAction.Break;
                }
                finally
                {
                    if (!hasConnected && clientPipe != null)
                    {
                        //Log.Verbose($"Failed to connect to browserName: {browserName}", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 116, "GetConnectedClientPipeAsync");
                        clientPipe.Dispose();
                    }
                }
            });
            if (out_connectedClientPipe == null)
            {
                //Log.Error($"[GetConnectedClientPipeAsync] Could not connect to {browserName} within timeout: {connectTimeoutMs}", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 125, "GetConnectedClientPipeAsync");
            }
            return out_connectedClientPipe;
        }

        public bool WaitServerPipe(BrowserName browserName, int connectTimeoutMs)
        {
            return Task.Run(async () => await WaitServerPipeAsync(browserName, connectTimeoutMs)).Result;
        }

        public Task<bool> WaitServerPipeAsync(BrowserName browserName, int connectTimeoutMs)
        {
            return Retry.WithTimeout(connectTimeoutMs, ClientConnectSleepTimeMs, delegate
            {
                string text = FindServerPipe(PipeNameBase, browserName, PlatformInfo.IsWindows);
                LoopAction loopAction = (string.IsNullOrEmpty(text) ? LoopAction.Continue : LoopAction.Break);
                return Task.FromResult(loopAction);
            });
        }

        public bool IsAnyServerPipeRunning(BrowserName browserName)
        {
            string text = FindServerPipe(PipeNameBase, browserName, PlatformInfo.IsWindows);
            return !string.IsNullOrEmpty(text);
        }

        public string FindServerPipe(string pipeNameBase, BrowserName browserName, bool isWindows)
        {
            return TimeTools.DoTimed("FindServerPipe", delegate
            {
                int currentSessionId = NativeService.GetCurrentSessionId();
                string pipeNameSearchPattern = GetPipeNameSearchPattern(pipeNameBase, browserName, currentSessionId);
                string pipeRoot = GetPipeRoot(isWindows);
                string[] files = Directory.GetFiles(pipeRoot, pipeNameSearchPattern);
                if (files == null || files.Length < 1)
                {
                    return (string)null;
                }
                string result = null;
                string[] array = files;
                foreach (string text in array)
                {
                    string text2 = TrimPipeRootIfNeeded(text, isWindows);
                    bool flag = false;
                    if (isWindows)
                    {
                        flag = true;
                    }
                    else if (IsServerProcessRunning(text2))
                    {
                        flag = true;
                    }
                    else
                    {
                        SafeFileDelete(text2);
                        //Log.Verbose("[FindServerPipeAsync]: Deleted leftover pipe file: " + text, "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 205, "FindServerPipe");
                    }
                    if (flag)
                    {
                        result = text2;
                        break;
                    }
                }
                return result;
            });
        }

        public static string GetPipeNameSearchPattern(string pipeNameBase, BrowserName browserName, int sessionId)
        {
            string text = BrowserNameAsString(browserName);
            return $"{pipeNameBase}.{text}.{sessionId}.*";
        }

        public static string GetServerPipeName(string pipeNameBase, BrowserName browserName, int sessionId, int instancePid, bool isWindows)
        {
            string text = BrowserNameAsString(browserName);
            string pipeRoot = GetPipeRoot(isWindows);
            string serverPipeName = $"{pipeRoot}{pipeNameBase}.{text}.{sessionId}.{instancePid}";
            return TrimPipeRootIfNeeded(serverPipeName, isWindows);
        }

        public static string TrimPipeRootIfNeeded(string serverPipeName, bool isWindows)
        {
            if (isWindows && serverPipeName.StartsWith("\\\\.\\pipe\\"))
            {
                return serverPipeName.Substring("\\\\.\\pipe\\".Length);
            }
            return serverPipeName;
        }

        public static string GetPipeRoot(bool isWindows)
        {
            return isWindows ? "\\\\.\\pipe\\" : "/tmp/";
        }

        private static string BrowserNameAsString(BrowserName name)
        {
            return name.ToString();
        }

        public static bool DeleteIfLeftoverPipeFile(string serverPipeName)
        {
            if (!IsServerProcessRunning(serverPipeName))
            {
                return SafeFileDelete(serverPipeName);
            }
            return false;
        }

        private static bool IsServerProcessRunning(string serverPipeName)
        {
            Process serverProcess = GetServerProcess(serverPipeName);
            if (serverProcess == null)
            {
                return false;
            }
            try
            {
                DateTime startTime = serverProcess.StartTime;
                DateTime creationTime = File.GetCreationTime(serverPipeName);
                return startTime.Ticks < creationTime.Ticks;
            }
            catch (Exception ex)
            {
                //Log.Warning(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 289, "IsServerProcessRunning");
            }
            return false;
        }

        private static Process GetServerProcess(string serverPipeName)
        {
            int num = ExtractServerPipePidFromPipeName(serverPipeName);
            if (num <= 0)
            {
                //Log.Error("Cannot extract process id from pipe name: " + serverPipeName, "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\Services\\Impl\\BrowserBridgePipeService.cs", 303, "GetServerProcess");
                return null;
            }
            try
            {
                return Process.GetProcessById(num);
            }
            catch (Exception)
            {
            }
            return null;
        }

        private static bool IsProcessRunning(int processId)
        {
            try
            {
                Process processById = Process.GetProcessById(processId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int ExtractServerPipePidFromPipeName(string serverPipeName)
        {
            int num = serverPipeName.LastIndexOf('.');
            if (num == -1)
            {
                return -1;
            }
            string text = serverPipeName.Substring(num + 1);
            if (int.TryParse(text, out var result))
            {
                return result;
            }
            return -1;
        }

        private static bool SafeFileDelete(string fileName)
        {
            try
            {
                File.Delete(fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
