using R3;

namespace Lemon.Automation.Domains
{
    public interface IUIAServiceFacade
    {
        IUIAElement GetDesktop();
        IEnumerable<IUIAElement> GetAllChildren(IUIAElement uiElement);
        IEnumerable<IUIAElement> GetAllChildren(string id);
    }
}
