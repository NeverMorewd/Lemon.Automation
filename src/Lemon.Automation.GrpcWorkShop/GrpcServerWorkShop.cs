using Grpc.Core;
using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop
{
    public class GrpcServerWorkShop
    {
        private readonly IEnumerable<IGrpcService> _services;
        private readonly IGrpcServer _server;
        private readonly ILogger _logger;

        public GrpcServerWorkShop(IEnumerable<IGrpcService> aServices, 
            IGrpcServer aServer, 
            ILogger<GrpcServerWorkShop> logger) 
        {
            _services = aServices;
            _server = aServer;
            _logger = logger;

            foreach (var service in _services)
            {
                service.Bind(_server.ServiceBinder);
            }
        }
        public void Start()
        {
            _logger.LogInformation($"GrpcServerWorkShop Start");
            _server.Start();
        }
        public void Stop() 
        {
            _logger.LogInformation($"GrpcServerWorkShop Stop");
            _server.Stop(); 
        }  
    }
}
