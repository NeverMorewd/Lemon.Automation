using Lemon.Automation.GrpcProvider.GrpcClients;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.App.UITracker.Services
{
    public class ElementInspectService
    {
        private readonly UIAutomationGrpcClientProvider _automationGrpcClientProvider;
        private readonly ElementHighlightService _elementHighlighter;
        private CancellationTokenSource? _cancellationSource;
        private readonly ILogger _logger;
        private bool _isTracking = false;
        public ElementInspectService(UIAutomationGrpcClientProvider automationGrpcClientProvider,
            ElementHighlightService elementHighlighter,
            ILogger<ElementInspectService> logger)
        {
            _logger = logger;
            _automationGrpcClientProvider = automationGrpcClientProvider;
            _elementHighlighter = elementHighlighter;
        }

        public async Task<Element> GetDesktop()
        {
            var ret = await _automationGrpcClientProvider.UIAutomationOperationClient.GetDesktopAsync(request: new GetDesktopRequest());
            return ret.Element;
        }

        public async Task<IEnumerable<Element>> GetAllChildren(Element element)
        {
            var ret = await _automationGrpcClientProvider.UIAutomationOperationClient.GetAllChildAsync(request: new GetAllChildRequest 
            {
                Element = element,
            });
            return ret.Element;
        }
    }
}
