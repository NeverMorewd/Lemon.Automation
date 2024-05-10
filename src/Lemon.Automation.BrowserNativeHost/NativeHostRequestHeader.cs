using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class NativeHostRequestHeader
    {
        public int TimeoutMs { get; set; }

        public string ClientPackageName { get; set; }

        public string ClientVersion { get; set; }

        public NativeHostRequestHeader()
        {
        }

        public NativeHostRequestHeader(int timeoutMs, string packageName, string version)
        {
            TimeoutMs = timeoutMs;
            ClientPackageName = packageName;
            ClientVersion = version;
        }
    }

    [JsonSerializable(typeof(NativeHostRequestHeader))]
    internal partial class NativeHostRequestHeader_SourceGenerationContext: JsonSerializerContext
    {
        
    }

}
