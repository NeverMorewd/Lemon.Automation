using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Framework.AutomationCore.Domains
{
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
        public string Name
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
    }
}
