using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class ResponseToExtension_Json
    {
        public int ReturnId { get; set; }

        public JsonElement ResponseData { get; set; }
    }

    [JsonSerializable(typeof(ResponseToExtension_Json))]
    internal partial class ResponseToExtension_SourceGenerationContext : JsonSerializerContext
    {
    }
}
