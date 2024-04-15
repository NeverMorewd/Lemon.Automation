using Grpc.Core;
using Lemon.Automation.GrpcProvider.GrpcClients;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace Lemon.Automation.App.UITracker.Track
{
    public class ElementTrackService
    {
        private readonly UIAutomationGrpcClientProvider _automationGrpcClientProvider;
        private readonly ElementHighlighter _elementHighlighter;
        public ElementTrackService(UIAutomationGrpcClientProvider automationGrpcClientProvider, ElementHighlighter elementHighlighter) 
        {
            _automationGrpcClientProvider = automationGrpcClientProvider;
            _elementHighlighter = elementHighlighter;
        }
        public async Task Start(CancellationToken cancellation)
        {
            var trackStreaming = _automationGrpcClientProvider.UIAutomationGrpcServiceClient.Tracking(null);

            await Task.Factory.StartNew(async () => 
            {
                while (await trackStreaming.ResponseStream.MoveNext(cancellation))
                {
                    var current = trackStreaming.ResponseStream.Current;
                    if (current.Element != null)
                    {
                        var elementRect = new Rectangle(current.Element.Left, current.Element.Top, current.Element.With, current.Element.Height);
                        _elementHighlighter.Highlight(elementRect);
                    }
                }
            }, cancellation,TaskCreationOptions.LongRunning, TaskScheduler.Default);


        }
    }
}
