using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class RequestFromExtension
    {
        public int RequestId { get; set; }

        public string FunctionCall { get; set; }

        public string DriverVersion { get; set; }

        public byte[] RequestData { get; set; }
    }
}
