using Grpc.Core;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop.GrpcServices
{
    public class UIAutomationGrpcService : UIAutomationService.UIAutomationServiceBase, IGrpcService
    {

        public override Task<TrackResponse> Track(TrackRequest request, 
            ServerCallContext context)
        {
            return base.Track(request, context);
        }

        public override Task Tracking(TrackRequest request, 
            IServerStreamWriter<TrackResponse> responseStream, 
            ServerCallContext context)
        {
            return base.Tracking(request, responseStream, context);
        }
        public void Bind(ServiceBinderBase serviceBinder)
        {
            UIAutomationService.BindService(serviceBinder, this);
        }

    }
}
