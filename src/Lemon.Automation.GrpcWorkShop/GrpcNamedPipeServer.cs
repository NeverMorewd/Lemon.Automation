using Grpc.Core;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop
{
    public class GrpcNamedPipeServer : IGrpcServer
    {
        public GrpcServerWorkShop WorkShop => throw new NotImplementedException();

        public ServiceBinderBase ServiceBinder => throw new NotImplementedException();

        public IEnumerable<IGrpcService>? Services => throw new NotImplementedException();

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
