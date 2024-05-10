using Lemon.Automation.BrowserNativeHost.Contracts;
using Lemon.Automation.BrowserNativeHost.Services;
using System.Buffers;
using System.Text.Json;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class BrowserExtensionConnector : IDisposable
    {
        private class ResponseTask
        {
            public Task<ResponseFromExtension> Task { get; set; }

            public int Id { get; set; }
        }

        private PipeProtocol m_pipeProtocol;

        private int m_lastRequestId = 0;

        private Mutex m_taskMutex = new Mutex();

        private Mutex m_sendMessageToExtensionMutex = new Mutex();

        private Dictionary<int, TaskCompletionSource<ResponseFromExtension>> m_mapRequestIdToResult = [];

        private CancellationTokenSource m_cancelSource = new CancellationTokenSource();

        private bool m_isDisposed = false;

        private bool m_isExtensionTraceEnabled = false;

        public BrowserExtensionConnector(Stream inputStream, Stream outputStream)
               : this(inputStream, outputStream, Service.Get<IWin32Registry>())
        {

        }
        public BrowserExtensionConnector(Stream inputStream, Stream outputStream, IWin32Registry registry)
        {
            m_pipeProtocol = new PipeProtocol(inputStream, outputStream);
            m_isExtensionTraceEnabled = IsExtensionTracingEnabled(registry);
        }

        public void Dispose()
        {
            if (!m_isDisposed)
            {
                m_isDisposed = true;
                m_cancelSource.Dispose();
                m_taskMutex.Dispose();
            }
        }

        public void RunExtensionLoop()
        {
            //Log.Info("Started", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 127, "RunExtensionLoop");
            try
            {
                while (true)
                {
                    ReadOnlySequence<byte> msgFromExtension = m_pipeProtocol.Read();
                    if (msgFromExtension.Length == 0)
                    {
                        break;
                    }
                    HandleMessageFromExtension_ThreadSafe(msgFromExtension);
                }
                //Log.Info("Received end-of-file; stopping...", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 137, "RunExtensionLoop");
            }
            catch (Exception ex)
            {
                //Log.Error(ex.Message, "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 147, "RunExtensionLoop");
            }
            m_cancelSource.Cancel();
            //Log.Info("Finished", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 153, "RunExtensionLoop");
        }

        private void HandleMessageFromExtension_ThreadSafe(ReadOnlySequence<byte> msgFromExtension)
        {
            try
            {
                using JsonDocument jsonDocument = JsonDocument.Parse(msgFromExtension);
                JsonElement rootElement = jsonDocument.RootElement;
                if (rootElement.TryGetProperty("FunctionCall", out var jsonElement))
                {
                    JsonElement property = rootElement.GetProperty("RequestId");
                    JsonElement property2 = rootElement.GetProperty("DriverVersion");
                    JsonElement property3 = rootElement.GetProperty("RequestData");
                    RequestFromExtension requestFromExtension = new RequestFromExtension
                    {
                        RequestId = property.GetInt32(),
                        FunctionCall = jsonElement.GetString(),
                        DriverVersion = property2.GetString(),
                        RequestData = SerializeJsonElement(property3)
                    };
                    HandleRequestFromExtension_ThreadSafe(requestFromExtension);
                }
                else
                {
                    JsonElement property4 = rootElement.GetProperty("RequestId");
                    JsonElement property5 = rootElement.GetProperty("ResponseCode");
                    JsonElement property6 = rootElement.GetProperty("ResponseData");
                    ResponseFromExtension taskResponse_ThreadSafe = new ResponseFromExtension
                    {
                        RequestId = property4.GetInt32(),
                        ResponseCode = (ExtensionResponseCode)property5.GetInt32(),
                        ResponseData = SerializeJsonElement(property6)
                    };
                    SetTaskResponse_ThreadSafe(taskResponse_ThreadSafe);
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 197, "HandleMessageFromExtension_ThreadSafe");
            }
        }

        private void HandleRequestFromExtension_ThreadSafe(RequestFromExtension requestFromExtension)
        {
            string text = "{}";
            if (requestFromExtension.FunctionCall == "IsTraceEnabled")
            {
                int num = (m_isExtensionTraceEnabled ? 1 : 0);
                text = $"{{\"isTraceEnabled\": {num}}}";
            }
            using JsonDocument jsonDocument = JsonDocument.Parse(text);
            ResponseToExtension_Json responseToExtension_Json = new ResponseToExtension_Json
            {
                ReturnId = requestFromExtension.RequestId,
                ResponseData = jsonDocument.RootElement
            };
            SendMessageToExtension_ThreadSafe(JsonSerializer.SerializeToUtf8Bytes(responseToExtension_Json, ResponseToExtension_SourceGenerationContext.Default.ResponseToExtension_Json));
        }

        private void SetTaskResponse_ThreadSafe(ResponseFromExtension response)
        {
            DoInTaskMutexScope(delegate
            {
                if (m_mapRequestIdToResult.TryGetValue(response.RequestId, out var taskCompletionSource))
                {
                    taskCompletionSource.SetResult(response);
                    m_mapRequestIdToResult.Remove(response.RequestId);
                }
                else
                {
                    //Log.Error("No pending task found with requestId: " + response.RequestId, "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 233, "SetTaskResponse_ThreadSafe");
                }
            });
        }

        public ResponseFromExtension SendRequest(ReadOnlySequence<byte> requestData, int timeoutMs)
        {
            try
            {
                using JsonDocument jsonDocument = JsonDocument.Parse(requestData);
                ResponseTask responseTask = AddResponseTask_ThreadSafe();
                RequestToExtension_Json requestToExtension_Json = new RequestToExtension_Json
                {
                    RequestId = responseTask.Id,
                    RequestData = jsonDocument.RootElement
                };
                SendMessageToExtension_ThreadSafe(JsonSerializer.SerializeToUtf8Bytes(requestToExtension_Json, RequestToExtension_SourceGenerationContext.Default.RequestToExtension_Json));
                if (responseTask.Task.Wait(timeoutMs, m_cancelSource.Token))
                {
                    return responseTask.Task.Result;
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 262, "SendRequest");
            }
            //Log.Warning("No extension response", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\BrowserExtensionConnector.cs", 264, "SendRequest");
            return new ResponseFromExtension();
        }

        private ResponseTask AddResponseTask_ThreadSafe()
        {
            int requestId = 0;
            TaskCompletionSource<ResponseFromExtension> completionSource = new TaskCompletionSource<ResponseFromExtension>();
            DoInTaskMutexScope(delegate
            {
                if (m_lastRequestId == 2147483647)
                {
                    m_lastRequestId = 0;
                }
                requestId = ++m_lastRequestId;
                m_mapRequestIdToResult[requestId] = completionSource;
            });
            return new ResponseTask
            {
                Task = completionSource.Task,
                Id = requestId
            };
        }

        private void DoInTaskMutexScope(Action action)
        {
            m_taskMutex.WaitOne();
            try
            {
                action();
            }
            finally
            {
                m_taskMutex.ReleaseMutex();
            }
        }

        private void SendMessageToExtension_ThreadSafe(byte[] msg)
        {
            m_sendMessageToExtensionMutex.WaitOne();
            try
            {
                m_pipeProtocol.Write(msg);
            }
            finally
            {
                m_sendMessageToExtensionMutex.ReleaseMutex();
            }
        }

        private static byte[] SerializeJsonElement(JsonElement jsonElement)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(memoryStream))
            {
                jsonElement.WriteTo(utf8JsonWriter);
                utf8JsonWriter.Flush();
            }
            return memoryStream.ToArray();
        }

        public static bool IsExtensionTracingEnabled(IWin32Registry registry)
        {
            if (registry == null)
            {
                return false;
            }
            int? num = registry.ReadIntValue(Win32RegistryHive.CurrentUser, "SOFTWARE\\UiPath", "WebExtensionTrace");
            if (!num.HasValue)
            {
                return false;
            }
            return num != 0;
        }

    }
}
