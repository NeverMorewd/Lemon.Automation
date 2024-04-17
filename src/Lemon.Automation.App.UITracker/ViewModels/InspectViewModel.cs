using Lemon.Automation.App.UITracker.Services;
using Lemon.Automation.Framework.Rx;
using Lemon.Automation.GrpcProvider.GrpcClients;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class InspectViewModel: RxViewModel
    {
        public InspectViewModel(UIAutomationGrpcClientProvider automationGrpcClientProvider,
            ElementHighlightService elementHighlighter,
            ILogger<InspectViewModel> logger) 
        {
            
        }
    }
}
