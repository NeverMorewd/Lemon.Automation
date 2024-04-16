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
using System.Diagnostics;

namespace Lemon.Automation.GrpcWorkShop.GrpcServices
{
    public class UIAutomationGrpcService : UIAutomationService.UIAutomationServiceBase, IGrpcService
    {
        private readonly IAutomationService _automationService;
        private readonly ILogger _logger;
        public UIAutomationGrpcService(IAutomationService  automationService, 
            ILogger<UIAutomationGrpcService> logger)
        {
            _automationService = automationService;
            _logger = logger;
            ObservableTracker.EnableTracking = true;
            ObservableTracker.EnableStackTrace = true;
            ObservableSystem.RegisterUnhandledExceptionHandler(exception => 
            {
                _logger.LogError(exception, "ObservableSystemExceptionHandle");
            });
        }
        public override Task<TrackResponse> Track(TrackRequest request, 
            ServerCallContext context)
        {
            return base.Track(request, context);
        }

        public override async Task Tracking(TrackRequest request, 
            IServerStreamWriter<TrackResponse> responseStream, 
            ServerCallContext context)
        {
            _logger.LogDebug($"Tracking start");
           var disposable = _automationService
                              .ObserveElementsFromCurrentPoint(context.CancellationToken)
                              .Select(ae =>
                              {
                                  if (!ae.Properties.BoundingRectangle.IsSupported 
                                      ||!ae.Properties.Name.IsSupported)
                                  {
                                      throw new Exception("Invalid Element");
                                  }
                                  var element = new Element
                                  {
                                      Height = ae.Properties.BoundingRectangle.ValueOrDefault.Height,
                                      With = ae.Properties.BoundingRectangle.ValueOrDefault.Width,
                                      Left = ae.Properties.BoundingRectangle.ValueOrDefault.Left,
                                      Top = ae.Properties.BoundingRectangle.ValueOrDefault.Top,
                                      Name = ae.Properties.Name.ValueOrDefault,
                                      NativeInfo = Struct.Parser.ParseJson(JsonConvert.SerializeObject(ae.ToNative())),
                                  };
                                  if (ae.Properties.ProcessId.IsSupported)
                                  {
                                      element.ProcessId = ae.Properties.ProcessId.ValueOrDefault;
                                  }
                                  if (ae.Properties.NativeWindowHandle.IsSupported)
                                  {
                                      element.ElementHandle = (int)ae.Properties.NativeWindowHandle.ValueOrDefault;
                                  }
                                  return element;
                              })
                              .ThrottleFirst(TimeSpan.FromMilliseconds(1))
                              .Subscribe(
                              onNext: async next => 
                              {
                                  _logger.LogDebug($"next:{next.Name}");
                                  await responseStream.WriteAsync(new TrackResponse
                                  {
                                       Element = next,
                                       Context = new ResponseContext
                                       {
                                          Message = "OK",
                                          Code = 0,
                                          End = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                                       }
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
                                          End = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                                      }
                                  });

                              },
                              onCompleted: async result => 
                              {
                                  _logger.LogInformation($"onCompleted:{result.IsSuccess};{result.IsFailure},{context.CancellationToken.IsCancellationRequested}");

                                  await responseStream.WriteAsync(new TrackResponse
                                  {
                                      Element = default,
                                      Context = new ResponseContext
                                      {
                                          Message = "Over",
                                          Code = 1,
                                          End = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                                      }
                                  });
                              });

            ObservableTracker.ForEachActiveTask(x =>
            {
                Console.WriteLine($"ObservableTracker:{x.TrackingId}:{x.StackTrace}");
            });
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(10);
            }
            disposable.Dispose();
            _logger.LogDebug($"Tracking Over");
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
