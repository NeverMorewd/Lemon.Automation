using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.GrpcProvider.GrpcClients
{
    public class UIAutomationGrpcClientProvider
    {
        private readonly NamedPipeChannel _channel;
        private readonly NamedPipeChannelOptions _channelOptions;
        private readonly UIAutomationService.UIAutomationServiceClient automationServiceClient;
        private readonly ILogger _logger;
        public UIAutomationGrpcClientProvider(IConnection connection, ILogger<UIAutomationGrpcClientProvider> logger)
        {
            _logger = logger;
            _channelOptions = new NamedPipeChannelOptions
            {
                ConnectionTimeout = connection.ConnectTimeout.GetValueOrDefault(),
            };
            _channel = new NamedPipeChannel(".", connection.ConnectionKey, _channelOptions);
            automationServiceClient = new UIAutomationService.UIAutomationServiceClient(_channel);

            _logger.LogDebug($"automationServiceClient:{connection.ConnectionKey}");
        }

        public UIAutomationService.UIAutomationServiceClient UIAutomationGrpcServiceClient 
        { 
            get
            {
                return automationServiceClient; 
            } 
        }


    }
}
