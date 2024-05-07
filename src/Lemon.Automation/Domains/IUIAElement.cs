using System.Drawing;

namespace Lemon.Automation.Domains
{
    public interface IUIAElement : IProtobufSerializable
    {
        public string? Name
        {
            get;
        }
        public string CacheId
        {
            get;
        }
        public string? ClassName
        {
            get;
        }
        public string? RootWindowClassName
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
        public string? Text
        {
            get;
        }
        public IEnumerable<IUIAElement> FindAllChildren();
    }
}
