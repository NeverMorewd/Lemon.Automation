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
        private readonly IAutomationServiceFacade _automationService;
        private readonly ILogger _logger;
        public UIAutomationGrpcService(IAutomationServiceFacade  automationService, 
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
            Observable<IUIElement> elementObservable = request.TrackType switch
            {
                TrackTypeEnum.MouseMove => _automationService.ObserveElementsByMouseMove(context.CancellationToken, TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault())),
                TrackTypeEnum.Continuous => _automationService.ObserveElementsFromCurrentPoint(context.CancellationToken, TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault())),
                _ => _automationService.ObserveElementsByMouseMove(context.CancellationToken, TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault())),
            };
            var disposable = elementObservable
                               .Do(x =>
                               {
                                   //_logger.LogDebug($"{x.Name}:{x.IsVisible}");
                               })
                               .Where(x => x != null && x.IsVisible)
                               .Select(ae =>
                               {
                                   var element = new Element
                                   {
                                       Height = ae.RegionRectangle.Height,
                                       With = ae.RegionRectangle.Width,
                                       Left = ae.RegionRectangle.Left,
                                       Top = ae.RegionRectangle.Top,
                                   };
                                   if (ae.ProcessId.HasValue)
                                   {
                                       element.ProcessId = ae.ProcessId.Value;
                                   }
                                   if (ae.ElementHandle.HasValue)
                                   {
                                       element.ElementHandle = ae.ElementHandle.Value;
                                   }
                                   if (string.IsNullOrEmpty(ae.Name))
                                   {
                                       element.Name = "none";
                                   }
                                   return element;
                               })
                               //R3:Throttle has been changed to Debounce, and Sample has been changed to ThrottleLast.
                               //https://github.com/Cysharp/R3/issues/193
                               //.Debounce(TimeSpan.FromMilliseconds(request.Interval ?? 1))
                               .Subscribe(
                               onNext: async next =>
                               {
                                   try
                                   {
                                       _logger.LogDebug($"next:{JsonConvert.SerializeObject(next)}");
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
                                   }
                                   catch (Exception ex) 
                                   {
                                       _logger.LogDebug(ex, "onNext");
                                   }
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
