using FlaUI.UIA3.Converters;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lemon.Automation.Framework.AutomationCore.Domains;
using Lemon.Automation.Globals;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using R3;

namespace Lemon.Automation.GrpcWorkShop.GrpcServices
{
    public class UIAutomationGrpcService : UIAutomationService.UIAutomationServiceBase, IGrpcService
    {
        private readonly IAutomationService _automationService;
        private readonly ILogger _logger;
        public UIAutomationGrpcService(IAutomationService  automationService, ILogger<UIAutomationGrpcService> logger)
        {
            _automationService = automationService;
            _logger = logger;
        }
        public override Task<TrackResponse> Track(TrackRequest request, 
            ServerCallContext context)
        {
            return base.Track(request, context);
        }

        public override Task Tracking(TrackRequest request, 
            IServerStreamWriter<TrackResponse> responseStream, 
            ServerCallContext context)
        {
           var disposable = _automationService.ObserveElementsFromCurrentPoint()
                              .Select(ae => new Element 
                              {
                                  Height = ae.BoundingRectangle.Height,
                                  With = ae.BoundingRectangle.Width,
                                  Left = ae.BoundingRectangle.Left,
                                  Top = ae.BoundingRectangle.Top,
                                  Name = ae.Name,
                                  WindowHandle = ae.Properties.NativeWindowHandle.ValueOrDefault.ToString(),
                                  ProcessId = ae.Properties.ProcessId,
                                  NativeInfo = Struct.Parser.ParseJson(JsonConvert.SerializeObject(ae.ToNative())),
                              })
                              .Subscribe(
                              onNext: async next => 
                              {
                                  await responseStream.WriteAsync(new TrackResponse
                                  {
                                       Element = next,
                                  });
                              },
                              onErrorResume: async error => 
                              {
                                  _logger.LogError(AppLogEventIds.CanIgnore, error, error.Message);

                                  await responseStream.WriteAsync(new TrackResponse
                                  {
                                      Element = default,
                                      Context = new ResponseContext
                                      {
                                          Message = error.Message,
                                          Code = -1,
                                          End = Timestamp.FromDateTime(DateTime.Now),
                                      }
                                  });

                              },
                              onCompleted: async result => 
                              {
                                  _logger.LogInformation($"result:{result.IsSuccess};{result.IsFailure}");

                                  await responseStream.WriteAsync(new TrackResponse
                                  {
                                      Element = default,
                                      Context = new ResponseContext
                                      {
                                          Message = "Over",
                                          Code = 1,
                                          End = Timestamp.FromDateTime(DateTime.Now),
                                      }
                                  });
                              });
            while (!context.CancellationToken.IsCancellationRequested)
            {
                //blocking
            }
            disposable.Dispose();
            return Task.CompletedTask;
        }

        public override Task<CaptureResponse> Capture(CaptureRequest request, ServerCallContext context)
        {
            return base.Capture(request, context);
        }

        public void Bind(ServiceBinderBase serviceBinder)
        {
            UIAutomationService.BindService(serviceBinder, this);
        }

    }
}
