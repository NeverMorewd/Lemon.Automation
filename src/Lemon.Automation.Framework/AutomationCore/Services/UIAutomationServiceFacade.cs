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
        private readonly Win32AutomationService _win32AutomationService;
        private readonly MSAAService _msaaService;
        private readonly ILogger _logger;
        public UIAutomationServiceFacade(ILogger<UIAutomationServiceFacade> logger,
            AutomationBase automationBase,
            Win32AutomationService win32AutomationService,
            MSAAService msaaService)
        {
            _logger = logger;
            _automationBase = automationBase;
            _win32AutomationService = win32AutomationService;
            _msaaService = msaaService;
        }
        public Observable<IUIElement> ObserveElementsFromCurrentPoint(CancellationToken cancellationToken, 
            TimeSpan interval,
            bool enableDeep) 
        {
            return Observable.CreateFrom(t =>
            {
                return ElementsFromCurrentPoint(cancellationToken, interval, enableDeep);
            });
        }
        private async IAsyncEnumerable<IUIElement> ElementsFromCurrentPoint([EnumeratorCancellation] CancellationToken cancellationToken, TimeSpan interval, bool enableDeep)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (interval.Milliseconds > 0)
                {
                    await Task.Delay(interval);
                }
                else
                {
                    await Task.Yield();
                }
                yield return ElementFromCurrentPoint(enableDeep);
            }
        }
        public IUIElement ElementFromCurrentPoint(bool enableDeep)
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
                if (enableDeep)
                {
                    var deepElement = GetClosestAndDeepestChildFromPoint();
                    if (deepElement != null)
                    {
                        return deepElement;
                    }
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
            TimeSpan interval,
            bool enableDeep)
        {
            return Observable.EveryValueChanged(this, _ => Mouse.Position, cancellationToken)
                .Select(p =>
                {
                    _logger.LogDebug($"point = ({p.X},{p.Y})");
                    return ElementFromCurrentPoint(enableDeep);
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
        public IUIElement GetClosestAndDeepestChildFromPoint()
        {
            var child = GetClosestAndDeepestChild(Mouse.Position);
            if (child != null)
            {
                return new Fla3UIElement(child);
            }
            else
            {
                return new Fla3UIElement(_automationBase.GetDesktop());
            }
        }
        private AutomationElement? GetClosestAndDeepestChild(Point point)
        {
            if (TryGetElementFromCurrentPointInternal(out AutomationElement? automation) && automation != null)
            {
                var treeWalker = _automationBase.TreeWalkerFactory.GetRawViewWalker();
                var children = GetDeepestChildren(treeWalker, automation);
                AutomationElement? closestElement = null;
                double minDistance = double.MaxValue;

                foreach (AutomationElement element in children)
                {
                    Rectangle elementRect = element.Properties.BoundingRectangle.ValueOrDefault;
                    if (elementRect.Contains(point))
                    {
                        Point elementCenter = new(elementRect.Left + elementRect.Width / 2,
                                                        elementRect.Top + elementRect.Height / 2);
                        double distance = Distance(point, elementCenter);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestElement = element;
                        }
                    }
                }
                return closestElement;
            }
            return null;
        }
        private static IEnumerable<AutomationElement> GetDeepestChildren(ITreeWalker treeWalker, AutomationElement targetElement)
        {
            var deepestChildren = new List<AutomationElement>();
            var queue = new Queue<AutomationElement>();
            queue.Enqueue(targetElement);

            while (queue.Count > 0)
            {
                deepestChildren.Clear();
                int levelSize = queue.Count;

                for (int i = 0; i < levelSize; i++)
                {
                    var current = queue.Dequeue();
                    deepestChildren.Add(current);

                    var child = treeWalker.GetFirstChild(current);
                    while (child != null)
                    {
                        queue.Enqueue(child);
                        child = treeWalker.GetNextSibling(child);
                    }
                    if (child == null)
                    {
                        // If no child was found, current is one of the deepest children
                        deepestChildren.Add(current);
                    }
                }
            }

            return deepestChildren;
        }

        private static IEnumerable<AutomationElement> GetAllChild(ITreeWalker treeWalker, AutomationElement targetElement)
        {
            var firstChildInFirstLevel = treeWalker.GetFirstChild(targetElement);
            var allChildrenInFirstLevel = GetAllSibling(treeWalker, firstChildInFirstLevel);
            return allChildrenInFirstLevel.SelectMany(child => GetAllChild(treeWalker, child));
        }

        private static IEnumerable<AutomationElement> GetAllSibling(ITreeWalker treeWalker, AutomationElement targetElement)
        {
            while (targetElement != null)
            {
                yield return targetElement;
                targetElement = treeWalker.GetNextSibling(targetElement);
            }
        }

        static double CalculateDistance(Point mousePos, Rectangle elementRect)
        {
            Point elementCenter = new(elementRect.Left + elementRect.Width / 2,
                                            elementRect.Top + elementRect.Height / 2);

            return Distance(mousePos, elementCenter);
        }

        static double Distance(Point p1, Point p2)
        {
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
