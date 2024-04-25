using System.Drawing;

namespace Lemon.Automation.Domains
{
    public interface IUIAElement
    {
        public string CacheId
        {
            get;
        }
        public string? ClassName
        {
            get;
        }
        public int? ProcessId
        {
            get;
        }
        public string? ControlType
        {
            get;
        }
        public bool IsAvailable
        {
            get;
        }
        public bool IsVisible
        {
            get;
        }
        public string? Name
        {
            get;
        }
        public Rectangle RegionRectangle
        {
            get;
        }
        public int? ElementHandle
        {
            get;
        }

        public string? WindowTitle
        {
            get;
        }
        public nint RootHandle
        {
            get;
        }

        public string? AdditionText
        {
            get;
        }
        public string FrameworkType
        {
            get;
        }
        public string ProcessName
        {
            get;
        }
        public IEnumerable<IUIAElement> FindAllChildren();
    }
}
