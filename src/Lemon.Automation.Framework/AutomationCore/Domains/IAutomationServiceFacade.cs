using R3;

namespace Lemon.Automation.Framework.AutomationCore.Domains
{
    public interface IAutomationServiceFacade
    {
        IUIElement ElementFromCurrentPoint(bool isEnableDeepTraversal);
        Observable<IUIElement> ObserveElementsFromCurrentPoint(CancellationToken cancellationToken,
            TimeSpan interval, 
            bool isEnableDeepTraversal);
        Observable<IUIElement> ObserveElementsByMouseMove(CancellationToken cancellationToken, 
            TimeSpan interval, 
            bool isEnableDeepTraversal);
        IEnumerable<IUIElement> GetAllChildFromPoint();
        IUIElement GetClosestAndDeepestChildFromPoint();
    }
}
