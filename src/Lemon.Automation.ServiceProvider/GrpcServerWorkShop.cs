using Grpc.Core;
using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop
{
    public class GrpcServerWorkShop
    {
        private readonly IEnumerable<IGrpcService> services;
        private readonly IGrpcServer server;

        public GrpcServerWorkShop(IEnumerable<IGrpcService> aServices, IGrpcServer aServer) 
        {
            services = aServices;
            server = aServer;
        }

        public void Process()
        {
            foreach (var service in services)
            {
                service.Bind(server.ServiceBinder);
            }
        }
    }
}
