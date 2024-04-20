using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using Lemon.Automation.Framework.AutomationCore.Domains;
using Lemon.Automation.Framework.AutomationCore.Models;
using Microsoft.Extensions.Logging;
using R3;
using System.Drawing;
using System.Runtime.CompilerServices;
//using System.Windows.Automation;

namespace Lemon.Automation.Framework.AutomationCore.Services
{
    public class UIAutomationServiceFacade : IAutomationServiceFacade
    {
        private readonly AutomationBase _automationBase;
        private readonly Win32AutomationSerivce _win32AutomationService;
        private readonly MSAAService _msaaService;
        private readonly ILogger _logger;
        public UIAutomationServiceFacade(ILogger<UIAutomationServiceFacade> logger,
            AutomationBase automationBase,
            Win32AutomationSerivce win32AutomationSerivce,
            MSAAService msaaService)
        {
            _logger = logger;
            _automationBase = automationBase;
            _win32AutomationService = win32AutomationSerivce;
            _msaaService = msaaService;
        }
        public Observable<IUIElement> ObserveElementsFromCurrentPoint(CancellationToken cancellationToken, 
            TimeSpan inerval) 
        {
            return Observable.CreateFrom(t =>
            {
                return ElementsFromCurrentPoint(cancellationToken, inerval);
            });
        }
        private async IAsyncEnumerable<IUIElement> ElementsFromCurrentPoint([EnumeratorCancellation] CancellationToken cancellationToken, TimeSpan inerval)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (inerval.Milliseconds > 0)
                {
                    await Task.Delay(inerval);
                }
                else
                {
                    await Task.Yield();
                }
                yield return ElementFromCurrentPoint();
            }
        }
        public IUIElement ElementFromCurrentPoint()
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
                    element = _automationBase.GetDesktop();
                }
                if (!element.Properties.BoundingRectangle.IsSupported)
                {
                    element = _automationBase.GetDesktop();
                }
                return new Fla3UIElement(element);
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    var handle = _win32AutomationService.WindowFromPoint(Mouse.Position);
                    return new Win32UIElement(() => _win32AutomationService, handle, nameof(UnauthorizedAccessException));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    return new Fla3UIElement(_automationBase.GetDesktop());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Fla3UIElement(_automationBase.GetDesktop());
            }
        }
        private bool TryGetElementFromCurrentPointInternal(out AutomationElement? automationElement)
        {
            try
            {
                automationElement = _automationBase.FromPoint(Mouse.Position);
                if (automationElement.Properties.ProcessId == Environment.ProcessId)
                {
                    automationElement = _automationBase.GetDesktop();
                }
                if (!automationElement.Properties.BoundingRectangle.IsSupported)
                {
                    automationElement = _automationBase.GetDesktop();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                automationElement = null;
                return false;
            }
        }

        public Observable<IUIElement> ObserveElementsByMouseMove(CancellationToken cancellationToken, 
            TimeSpan inerval)
        {
            return Observable.EveryValueChanged(this, _ => Mouse.Position, cancellationToken)
                .Select(p =>
                {
                    _logger.LogDebug($"point = ({p.X},{p.Y})");
                    return ElementFromCurrentPoint();
                });
        }

        public IEnumerable<IUIElement> GetAllChildFromPoint()
        {
            if (TryGetElementFromCurrentPointInternal(out AutomationElement? automation) && automation != null)
            {
                var treeWalker = _automationBase.TreeWalkerFactory.GetRawViewWalker();
                return GetAllChild(treeWalker, automation).Select(element=>new Fla3UIElement(element));
            }
            return [];
        }

        private static IEnumerable<AutomationElement> GetAllChild(ITreeWalker treeWalker, AutomationElement targetElement)
        {
            var firstChildInFirstLevel = treeWalker.GetFirstChild(targetElement);
            var allChildrenInFirstLevel = GetAllSibling(treeWalker, firstChildInFirstLevel);
            return allChildrenInFirstLevel.SelectMany(child => GetAllChild(treeWalker, targetElement));
        }

        private static IEnumerable<AutomationElement> GetAllSibling(ITreeWalker treeWalker, AutomationElement targetElement)
        {
            while (targetElement != null)
            {
                yield return targetElement;
                targetElement = treeWalker.GetNextSibling(targetElement);
            }
        }
    }
}
