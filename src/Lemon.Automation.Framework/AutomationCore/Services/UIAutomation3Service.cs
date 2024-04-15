using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using Lemon.Automation.Domains;
using Lemon.Automation.Framework.AutomationCore.Domains;
using Newtonsoft.Json.Linq;
using R3;
//using System.Windows.Automation;

namespace Lemon.Automation.Framework.AutomationCore.Services
{
    public class UIAutomation3Service : IAutomationService
    {
        private readonly AutomationBase _automationBase;
        public UIAutomation3Service()
        {
            _automationBase = new UIA3Automation();
        }
        public Observable<JObject> ObserveElementObjectsFromCurrentPoint()
        {
            return ObserveElementsFromCurrentPoint().Select(JObject.FromObject);
        }
        public Observable<AutomationElement> ObserveElementsFromCurrentPoint() 
        {
            return Observable.CreateFrom(t =>
            {
                return ElementsFromCurrentPoint();
            });
        }

        private async IAsyncEnumerable<AutomationElement> ElementsFromCurrentPoint()
        {
             while (true)
             {
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
            var element = _automationBase.FromPoint(Mouse.Position);
            if (element.Properties.ProcessId == Environment.ProcessId)
            {
                return  _automationBase.GetDesktop();
            }
            return element;
        }
    }
}
