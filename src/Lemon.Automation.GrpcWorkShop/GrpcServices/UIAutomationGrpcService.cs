using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lemon.Automation.Framework.AutomationCore.Domains;
using Lemon.Automation.Framework.AutomationCore.Models;
using Lemon.Automation.Framework.Extensions;
using Lemon.Automation.Globals;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoBuf;
using R3;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;

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

        public async override Task<UIAutomationProxyResponse> UIAutomationProxy(UIAutomationProxyRequest request, ServerCallContext context)
        {
            return await base.UIAutomationProxy(request, context);
        }
        public async override Task<GetDesktopResponse> GetDesktop(GetDesktopRequest request, ServerCallContext context)
        {
            _logger.LogDebug($"GetDesktop start");
            var desktopElement = _automationService.GetDesktop();
            var uiElement = Transform(desktopElement);
            return await Task.FromResult(new GetDesktopResponse 
            {
                Element = uiElement,
                Context = new ResponseContext
                {
                    Code = 0, 
                }
            });
        }

        public override Task<GetAllChildResponse> GetAllChild(GetAllChildRequest request, ServerCallContext context)
        {
            _logger.LogDebug($"GetAllChild start");
            //request.Element.na
            var buffer = request.Element.NativeUiaObject.ToArray();
            var element = FlaUI3Element.Deserialize(buffer);

            if (element != null)
            {
                var eles = _automationService.GetAllChildren(element);

                var uis = eles.Select(Transform);
                GetAllChildResponse response = new();
                response.Element.AddRange(uis);
                response.Context = new ResponseContext
                {
                    Code = 0,
                };
                return Task.FromResult(response);
            }
            throw new Exception("IUIElement");
        }

        public override async Task Tracking(TrackRequest request,
            IServerStreamWriter<TrackResponse> responseStream,
            ServerCallContext context)
        {
            _logger.LogDebug($"Tracking start");
            Observable<IUIElement> elementObservable = request.TrackType switch
            {
                TrackTypeEnum.MouseMove => _automationService.ObserveElementsByMouseMove(context.CancellationToken, 
                                            TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault()),
                                            request.EnableDeep),

                TrackTypeEnum.Continuous => _automationService.ObserveElementsFromCurrentPoint(context.CancellationToken, 
                                            TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault()),
                                            request.EnableDeep),

                _ => _automationService.ObserveElementsByMouseMove(context.CancellationToken, 
                                            TimeSpan.FromMilliseconds(request.Interval.GetValueOrDefault()),
                                            request.EnableDeep),
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

        public override Task<CaptureResponse> Capture(CaptureRequest request, ServerCallContext context)
        {
            return base.Capture(request, context);
        }

        public void Bind(ServiceBinderBase serviceBinder)
        {
            UIAutomationService.BindService(serviceBinder, this);
        }

        private Element Transform(IUIElement uiElement)
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
                element.ElementHandle = uiElement.ElementHandle.Value;
            }
            if (string.IsNullOrEmpty(uiElement.Name))
            {
                element.Name = "none";
            }
            try
            {
                if (uiElement is FlaUI3Element flauiElement)
                {
                    var bytes = FlaUI3Element.Serialize(flauiElement);
                    element.NativeUiaObject = ByteString.CopyFrom(bytes);
                }


                //var obj = ProtobufTest.Deserialize(bytes);

            }
            catch (Exception ex) 
            {
                _logger.LogDebug(ex,"");
            }
            return element;
        }


    }
}
