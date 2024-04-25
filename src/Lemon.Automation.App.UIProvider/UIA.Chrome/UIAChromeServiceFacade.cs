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
            return false;
        }
    }
}
