using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using Lemon.Automation.Domains;
using Lemon.Automation.Framework.AutomationCore.Domains;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using R3;
using System.Runtime.CompilerServices;
using System.Threading;
//using System.Windows.Automation;

namespace Lemon.Automation.Framework.AutomationCore.Services
{
    public class UIAutomation3Service : IAutomationService
    {
        private readonly AutomationBase _automationBase;
        private readonly ILogger _logger;
        public UIAutomation3Service(ILogger<UIAutomation3Service> logger)
        {
            _logger = logger;
            _automationBase = new UIA3Automation();
        }
        public Observable<JObject> ObserveElementObjectsFromCurrentPoint(CancellationToken cancellationToken)
        {
            return ObserveElementsFromCurrentPoint(cancellationToken).Select(JObject.FromObject);
        }
        public Observable<AutomationElement> ObserveElementsFromCurrentPoint(CancellationToken cancellationToken) 
        {
            return Observable.CreateFrom(t =>
            {
                return ElementsFromCurrentPoint(cancellationToken);
            });
        }

        private async IAsyncEnumerable<AutomationElement> ElementsFromCurrentPoint([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await Task.Yield();
                yield return ElementFromCurrentPoint();
            }
        }
        public AutomationElement ElementFromCurrentPoint()
        {
            /* FlaUI Mouse.Position
            {
                get
                {
                    User32.GetCursorPos(out var point);
                    return new Point(point.X, point.Y);
                }
                set
                {
                    User32.SetCursorPos(value.X, value.Y);
                    // There is a bug that in a multi-monitor scenario with different sizes,
                    // the mouse is only moved to x=0 on the target monitor.
                    // In that case, just redo the move a 2nd time and it works
                    // as the mouse is on the correct monitor alreay.
                    // See https://stackoverflow.com/questions/58753372/winapi-setcursorpos-seems-like-not-working-properly-on-multiple-monitors-with-di
                    User32.GetCursorPos(out var point);
                    if (point.X != value.X || point.Y != value.Y)
                    {
                        User32.SetCursorPos(value.X, value.Y);
                    }
                }
            }
            */

            try
            {
                var element = _automationBase.FromPoint(Mouse.Position);
                if (element.Properties.ProcessId == Environment.ProcessId)
                {
                    return _automationBase.GetDesktop();
                }
                if (!element.Properties.BoundingRectangle.IsSupported)
                {
                    return _automationBase.GetDesktop();
                }
                return element;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
                //return _automationBase.GetDesktop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
