using Lemon.Automation.App.UIProvider.UIA.Windows;
using Lemon.Automation.Domains;

namespace Lemon.Automation.App.UIProvider.UIA.Commons
{
    public class TrackEvidence : ITrackEvidence
    {
        private readonly Win32Element _window;
        public TrackEvidence(Win32Element win32Element, Point cursor) 
        {
            _window = win32Element;
            Cursor = cursor;
        }
        public Point Cursor
        {
            get;
            private set;
        }

        public string? RootWindowClassName => throw new NotImplementedException();

        public string? ProcessName => _window.ProcessName;

        public string? RootWindowTitle => _window.WindowTitle;

        public nint RootWindow => _window.RootHandle;

        public string? ClassName =>  _window.ClassName;
    }
}
