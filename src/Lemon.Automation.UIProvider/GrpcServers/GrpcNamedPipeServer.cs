using Grpc.Core;
using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;

namespace Lemon.Automation.UIProvider.GrpcServers
{
    public class GrpcNamedPipeServer : IGrpcServer
    {
        private readonly NamedPipeServer _server;

        public GrpcNamedPipeServer(IConnection connection)
        {
            _server = new NamedPipeServer(connection.ConnectionKey);
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
