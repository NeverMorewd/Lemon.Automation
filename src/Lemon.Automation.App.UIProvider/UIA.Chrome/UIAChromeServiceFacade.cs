using Lemon.Automation.Domains;

namespace Lemon.Automation.App.UIProvider.UIA.Chrome
{
    internal class UIAChromeServiceFacade : IUIATracker, IUIAServiceFacade
    {
        public IUIAElement ElementFromPoint(int aX, int Y, bool enableDeep)
        {
            throw new NotImplementedException();
        }

        public bool Examine(ITrackEvidence evidence)
        {
            if (evidence.ProcessName == "chrome")
            {
                if (evidence.ClassName == "Chrome_RenderWidgetHostHWND")
                {
                    return true;
                }
                if (evidence.ClassName == "Chrome_WidgetWin_1")
                {
                    // todo check connection
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<IUIAElement> GetAllChildren(IUIAElement uiElement)
        {
            throw new NotImplementedException();
        }

        public IUIAElement GetDesktop()
        {
            throw new NotImplementedException();
        }
    }
}
