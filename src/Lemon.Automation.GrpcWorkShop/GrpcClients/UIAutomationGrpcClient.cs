using Grpc.Core;
using Lemon.Automation.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcProvider.GrpcClients
{
    public class UIAutomationGrpcClient : UIAutomationService.UIAutomationServiceClient
    {
        public UIAutomationGrpcClient(ChannelBase aChannel) : base(aChannel)
        {
            
        }
    }
}
