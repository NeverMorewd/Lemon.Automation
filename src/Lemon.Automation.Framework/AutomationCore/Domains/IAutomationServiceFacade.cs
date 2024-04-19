using R3;

namespace Lemon.Automation.Framework.AutomationCore.Domains
{
    public interface IAutomationServiceFacade
    {
        IUIElement ElementFromCurrentPoint();
        Observable<IUIElement> ObserveElementsFromCurrentPoint(CancellationToken cancellationToken,TimeSpan inerval);
        Observable<IUIElement> ObserveElementsByMouseMove(CancellationToken cancellationToken, TimeSpan inerval);
        IEnumerable<IUIElement> GetAllChildFromPoint();
    }
}
