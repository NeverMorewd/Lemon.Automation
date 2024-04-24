using Lemon.Automation.Framework.AutomationCore.Models;
using ProtoBuf;
using System.Drawing;

namespace Lemon.Automation.Framework.AutomationCore.Domains
{
    [ProtoContract]
    [ProtoInclude(7, typeof(FlaUI3Element))]
    public interface IUIElement
    {
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
        public int? RootHandle
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

        public IEnumerable<IUIElement> FindAllChildren();
    }
}
