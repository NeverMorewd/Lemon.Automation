using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lemon.Automation.Globals;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Lemon.Automation.GrpcWorkShop.GrpcServices
{
    public class BeepGrpcService : BeepService.BeepServiceBase, IGrpcService
    {
        private readonly ILogger _logger;
        public BeepGrpcService(ILogger<BeepGrpcService> logger) 
        {
            _logger = logger;
            _logger.LogDebug("BeepGrpcService start");
        }
        public override async Task<HeartBeatPack> Beep(HeartBeatPack request, 
            ServerCallContext context)
        {
            try
            {
                foreach (var anyThing in request.AnyThings)
                {
                    if (anyThing.CalculateSize() > 0)
                    {
                        _logger.LogDebug($"Beep:{JsonSerializer.Serialize(anyThing)}");
                    }
                    else
                    {
                        _logger.LogDebug($"Beep:empty pack");
                    }
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(AppLogEventIds.CanIgnore, ex, ex.Message);
            }

            var pack = new  HeartBeatPack();
            pack.AnyThings.Add(Any.Parser.ParseJson("{\"beep\":0}"));
            return pack;
        }

        public override Task Beeping(IAsyncStreamReader<HeartBeatPack> requestStream, 
            IServerStreamWriter<HeartBeatPack> responseStream, 
            ServerCallContext context)
        {
            return base.Beeping(requestStream, responseStream, context);
        }

        public void Bind(ServiceBinderBase serviceBinder)
        {
            BeepService.BindService(serviceBinder, this);
        }
    }
}
