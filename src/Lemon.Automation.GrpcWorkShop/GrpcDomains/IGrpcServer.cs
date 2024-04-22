using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop.GrpcDomains
{
    public interface IGrpcServer
    {
        public void Start();
        public void Stop();
        public ServiceBinderBase ServiceBinder { get; }
    }
}
