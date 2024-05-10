using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class ResponseFromExtension
    {
        public int RequestId { get; set; }

        public ExtensionResponseCode ResponseCode { get; set; } = ExtensionResponseCode.Error_Timeout;


        public byte[] ResponseData { get; set; } = Array.Empty<byte>();

    }

    internal enum ExtensionResponseCode
    {
        Error_Timeout,
        Error_NeedsScripts,
        Success,
        Error_StudioAlreadyConnected,
        UnknownError
    }

}
