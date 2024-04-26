using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using static Lemon.Automation.Protos.UIAutomationOperationService;

namespace Lemon.Automation.GrpcProvider.GrpcClients
{
    public class UIAutomationGrpcClientProvider
    {
        private readonly NamedPipeChannel _channel;
        private readonly NamedPipeChannelOptions _channelOptions;
        private readonly UIAutomationTrackService.UIAutomationTrackServiceClient automationTrackServiceClient;
        private readonly UIAutomationOperationServiceClient automationOperationServiceClient;
        private readonly ILogger _logger;
        public UIAutomationGrpcClientProvider(IConnection connection, 
            ILogger<UIAutomationGrpcClientProvider> logger)
        {
            _logger = logger;
            _channelOptions = new NamedPipeChannelOptions
            {
                ConnectionTimeout = connection.ConnectTimeout.GetValueOrDefault(),
            };
            _channel = new NamedPipeChannel(".", connection.ConnectionKey, _channelOptions);
            automationTrackServiceClient = new UIAutomationTrackService.UIAutomationTrackServiceClient(_channel);
            automationOperationServiceClient = new UIAutomationOperationServiceClient(_channel);
            _logger.LogDebug($"automationServiceClient:{connection.ConnectionKey}");
        }

        public UIAutomationTrackService.UIAutomationTrackServiceClient UIAutomationTrackClient 
        { 
            get
            {
                return automationTrackServiceClient; 
            } 
        }

        public UIAutomationOperationServiceClient UIAutomationOperationClient
        {
            get 
            { 
                return automationOperationServiceClient; 
            }
        }



    }
}
