using Grpc.Core;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcDotNetNamedPipes;

namespace Lemon.Automation.UIProvider
{
    public class GrpcNamedPipeServer : IGrpcServer
    {
        private readonly NamedPipeServer _server;

        public GrpcNamedPipeServer(string aPipeName)
        {
            _server = new NamedPipeServer(aPipeName);
        }

        public ServiceBinderBase ServiceBinder => _server.ServiceBinder;

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Kill();
        }
    }
}
