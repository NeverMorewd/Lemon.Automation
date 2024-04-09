using Grpc.Core;
using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.App.UIProvider.GrpcServers
{
    public class GrpcNamedPipeServer : IGrpcServer
    {
        private readonly NamedPipeServer _server;
        private readonly ILogger _logger;
        private readonly IConnection _connection;

        public GrpcNamedPipeServer(IConnection connection, 
            ILogger<GrpcNamedPipeServer> logger)
        {
            _connection = connection;
            _server = new NamedPipeServer(_connection.ConnectionKey);
            _logger = logger;

            _server.Error += Server_Error;
        }

        private void Server_Error(object? sender, NamedPipeErrorEventArgs e)
        {
            if (e.Error is ObjectDisposedException)
            {
                //ignore
                return;
            }
            else
            {
                _logger.LogError(e.Error, "NamedPipeServerError");
                return;
            }
        }

        public ServiceBinderBase ServiceBinder => _server.ServiceBinder;

        public void Start()
        {
            _server.Start();
            _logger.LogInformation($"{nameof(GrpcNamedPipeServer)} started:{_connection.ConnectionKey}");
        }

        public void Stop()
        {
            _server.Kill();
        }
    }
}
