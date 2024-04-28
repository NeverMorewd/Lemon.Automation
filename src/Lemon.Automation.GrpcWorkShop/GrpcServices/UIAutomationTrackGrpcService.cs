using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lemon.Automation.Domains;
using Lemon.Automation.Globals;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using R3;

namespace Lemon.Automation.GrpcWorkShop.GrpcServices
{
    public class UIAutomationTrackGrpcService : UIAutomationTrackService.UIAutomationTrackServiceBase, IGrpcService
    {
        private readonly IUIATrackService _trackService;
        private readonly ILogger _logger;
        public UIAutomationTrackGrpcService(IUIATrackService trackService, 
            ILogger<UIAutomationTrackGrpcService> logger)
        {
            _trackService = trackService;
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
            Observable<IUIAElement> elementObservable = request.TrackType switch
            {
                TrackTypeEnum.MouseMove => _trackService.ObserveElementsByMouseMove(
                                            TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault()),
                                            request.EnableDeep,
                                            context.CancellationToken),

                TrackTypeEnum.Continuous => _trackService.ObserveElementsFromCurrentPoint(
                                            TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault()),
                                            request.EnableDeep,
                                            context.CancellationToken),

                _ => _trackService.ObserveElementsByMouseMove(
                                            TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault()),
                                            request.EnableDeep,
                                            context.CancellationToken),
            };
            var disposable = elementObservable
                               .Do(x =>
                               {
                                   //_logger.LogDebug($"{x.Name}:{x.IsVisible}");
                               })
                               .Where(x => x != null && x.IsVisible)
                               .Select(Transform)
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


        public void Bind(ServiceBinderBase serviceBinder)
        {
            UIAutomationTrackService.BindService(serviceBinder, this);
        }

        private Element Transform(IUIAElement uiElement)
        {
            var element = new Element
            {
                Height = uiElement.RegionRectangle.Height,
                With = uiElement.RegionRectangle.Width,
                Left = uiElement.RegionRectangle.Left,
                Top = uiElement.RegionRectangle.Top,
            };
            if (uiElement.ProcessId.HasValue)
            {
                element.ProcessId = uiElement.ProcessId.Value;
            }
            if (uiElement.ElementHandle.HasValue)
            {
                element.Handle = uiElement.ElementHandle.Value;
            }
            if (string.IsNullOrEmpty(uiElement.Name))
            {
                element.Name = "none";
            }

            return element;
        }


    }
}
