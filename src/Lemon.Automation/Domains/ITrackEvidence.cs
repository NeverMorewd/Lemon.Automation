using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Domains
{
    public interface ITrackEvidence
    {
        public nint RootWindow
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
        public Point Cursor
        {
            get;
        }
        public string? ProcessName
        {
            get;
        }
        public string? RootWindowTitle
        {
            get;
        }
    }
}
