using Newtonsoft.Json.Linq;
using R3;

namespace Lemon.Automation.Framework.AutomationCore.Domains
{
    public interface IAutomationServiceFacade
    {
        IUIElement ElementFromCurrentPoint();
        Observable<JObject> ObserveElementObjectsFromCurrentPoint(CancellationToken cancellationToken);
        Observable<IUIElement> ObserveElementsFromCurrentPoint(CancellationToken cancellationToken);
    }
}
