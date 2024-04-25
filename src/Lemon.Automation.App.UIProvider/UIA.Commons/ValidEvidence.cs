using Lemon.Automation.Domains;

namespace Lemon.Automation.App.UIProvider.UIA.Commons
{
    public class ValidEvidence
    {
        private readonly nint _handle;
        private readonly IUIATracker _tracker;
        public ValidEvidence(nint handle, IUIATracker tracker) 
        {
            _handle = handle;
            _tracker = tracker;
        }
        public IUIATracker Tracker
        {
            get
            {
                return _tracker;
            }
        }
        public nint Handle
        {
            get
            {
                return _handle;
            }
        }
    }
}
