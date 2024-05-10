using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class RequestToExtension_Json
    {
        public int RequestId { get; set; }

        public JsonElement RequestData { get; set; }
    }

    [JsonSerializable(typeof(RequestToExtension_Json))]
    internal partial class RequestToExtension_SourceGenerationContext: JsonSerializerContext
    {
        
    }
}
