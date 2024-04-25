namespace Lemon.Automation.Domains
{
    public interface IUIATracker
    {
        public IUIAElement ElementFromPoint(int aX, int Y, bool enableDeep);
        public bool Examine(ITrackEvidence evidence);
    }
}
