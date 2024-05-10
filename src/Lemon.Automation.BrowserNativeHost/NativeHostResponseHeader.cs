using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class NativeHostResponseHeader
    {
        public ExtensionResponseCode ResponseCode { get; set; }
    }

    [JsonSerializable(typeof(NativeHostResponseHeader))]
    internal partial class NativeHostResponseHeader_SourceGenerationContext : JsonSerializerContext
    {

    }
}
