using Grpc.Core;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop
{
    public class GrpcNamedPipeServerBuilder : IGrpcServerBuilder
    {
        private readonly IGrpcServer _server;
        public GrpcNamedPipeServerBuilder(string aPipeName) 
        {

        }
        public Func<GrpcServerWorkShop, IGrpcServerBuilder> AddWorkShop => throw new NotImplementedException();

        public Func<ServiceBinderBase, IGrpcServerBuilder> AddServiceBinder => throw new NotImplementedException();

        public Func<IEnumerable<IGrpcService>, IGrpcServerBuilder> AddServices => throw new NotImplementedException();

        public IGrpcServer Build()
        {
            throw new NotImplementedException();
        }
    }
}
