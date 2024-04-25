using FlaUI.Core.Input;
using Lemon.Automation.App.UIProvider.UIA.Commons;
using Lemon.Automation.App.UIProvider.UIA.Windows;
using Lemon.Automation.Domains;
using Microsoft.Extensions.Logging;
using R3;
using System.Runtime.CompilerServices;

namespace Lemon.Automation.App.UIProvider.UIA.Services
{
    public class UIATrackService : IUIATrackService
    {
        public readonly Win32AutomationService _win32AutomationService;
        public readonly IEnumerable<IUIATracker> _trackers;
        public readonly ILogger _logger;
        public UIATrackService(IEnumerable<IUIATracker> trackers, 
            Win32AutomationService win32AutomationService,
            ILogger<UIATrackService> logger)
        {
            _trackers = trackers;
            _win32AutomationService = win32AutomationService;
            _logger = logger;
        }
        public IUIAElement ElementFromPoint(int aX, int aY, bool isEnableDeepTraversal)
        {
            Point cursor = new(aX, aY);
            var handle = _win32AutomationService.WindowFromPoint(cursor);
            Win32Element win32Element = new(() => _win32AutomationService, handle);
            TrackEvidence evidence = new(win32Element, cursor);
            var targetTracker = ResolveTracker(evidence);
            return targetTracker.ElementFromPoint(aX, aY, isEnableDeepTraversal);
        }


        public Observable<IUIAElement> ObserveElementsByMouseMove(TimeSpan interval, 
            bool isEnableDeepTraversal, 
            CancellationToken cancellationToken)
        {
            return Observable.EveryValueChanged(this, _ => Mouse.Position, cancellationToken)
               .Select(p =>
               {
                   _logger.LogDebug($"point = ({p.X},{p.Y})");
                   return ElementFromPoint(p.X, p.Y, isEnableDeepTraversal);
               });
        }

        public Observable<IUIAElement> ObserveElementsFromCurrentPoint(TimeSpan interval, 
            bool isEnableDeepTraversal, 
            CancellationToken cancellationToken)
        {
            return Observable.CreateFrom(t =>
            {
                return ElementsFromCurrentPointInternal(interval, isEnableDeepTraversal, cancellationToken);
            });
        }

        private IUIATracker ResolveTracker(TrackEvidence trackEvidence)
        {
            foreach (var tracker in _trackers)
            {
                if (tracker.Examine(trackEvidence))
                {
                    return tracker;
                }
            }
            return _trackers.Last();
        }
        private async IAsyncEnumerable<IUIAElement> ElementsFromCurrentPointInternal(
            TimeSpan interval, 
            bool enableDeep, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (interval.Milliseconds > 0)
                {
                    await Task.Delay(interval, cancellationToken);
                }
                Point cursor = Mouse.Position;
                var handle = _win32AutomationService.WindowFromPoint(cursor);
                Win32Element win32Element = new(() => _win32AutomationService, handle);
                TrackEvidence evidence = new(win32Element, cursor);
                var targetTracker = ResolveTracker(evidence);
                yield return targetTracker.ElementFromPoint(cursor.X, cursor.Y, enableDeep);
            }
        }

    }
}
