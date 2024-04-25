using Lemon.Automation.Domains;

namespace Lemon.Automation.App.UIProvider.UIA.Chrome
{
    internal class UIAChromeServiceFacade : IUIATracker
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
    }
}
