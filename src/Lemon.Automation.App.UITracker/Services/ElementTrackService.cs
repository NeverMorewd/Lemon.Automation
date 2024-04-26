﻿using Lemon.Automation.Globals;
using Lemon.Automation.GrpcProvider.GrpcClients;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.App.UITracker.Services
{
    public class ElementTrackService
    {
        private readonly UIAutomationGrpcClientProvider _automationGrpcClientProvider;
        private readonly ElementHighlightService _elementHighlighter;
        private CancellationTokenSource? _cancellationSource;
        private readonly ILogger _logger;
        private bool _isTracking = false;
        public ElementTrackService(UIAutomationGrpcClientProvider automationGrpcClientProvider,
            ElementHighlightService elementHighlighter,
            ILogger<ElementTrackService> logger) 
        {
            _logger = logger;
            _automationGrpcClientProvider = automationGrpcClientProvider;
            _elementHighlighter = elementHighlighter;
        }
        public async Task<bool> Start(TrackTypeEnum trackType = TrackTypeEnum.MouseMove)
        {
            if (_isTracking)
            {
                return _isTracking;
            }
            _isTracking = true;
            _cancellationSource = new CancellationTokenSource();
            _elementHighlighter.Enable();
            TrackRequest trackRequest = new()
            {
                Interval = 1,
                TrackType = trackType,
                EnableDeep = false,
            };
            var trackStreaming = _automationGrpcClientProvider.UIAutomationTrackClient.Tracking(trackRequest, cancellationToken: _cancellationSource.Token);
            try
            {
                while (await trackStreaming.ResponseStream.MoveNext(_cancellationSource.Token))
                {
                    try
                    {
                        var current = trackStreaming.ResponseStream.Current;
                        if (current.Context.Code == 0)
                        {
                            if (current.Element != null)
                            {
                                if (current.Element.ProcessId == Environment.ProcessId)
                                {
                                    continue;
                                }
                                var elementRect = new Rectangle(current.Element.Left, current.Element.Top, current.Element.With, current.Element.Height);
                                _elementHighlighter.Highlight(elementRect);
                            }
                        }
                        else
                        {
                            await Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(AppLogEventIds.GrpcClient, ex, ex.Message);
                        if (ex.InnerException != null)
                        {
                            _logger.LogError(AppLogEventIds.GrpcClient, ex.InnerException, ex.InnerException.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //ignore
                _logger.LogWarning(ex,"Tracking over with error");
                if (ex.InnerException != null)
                {
                    _logger.LogWarning(AppLogEventIds.GrpcClient, ex.InnerException, ex.InnerException.Message);
                }
            }
            _isTracking = false;
            return _isTracking;
        }

        public Task Stop()
        {
            if (_isTracking)
            {
                _cancellationSource.Cancel();
                _elementHighlighter.Disable();
            }
            return Task.CompletedTask;
        }
    }
}
