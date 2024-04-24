using Lemon.Automation.Framework.AutomationCore.Domains;
using Lemon.Automation.Framework.AutomationCore.Services;
using System.Drawing;

namespace Lemon.Automation.Framework.AutomationCore.Models
{
    public class Win32UIElement : IUIElement
    {
        private readonly Func<Win32AutomationService> _win32AutomationSerivceGetter;
        private Win32AutomationService? _win32AutomationSerivce;
        private readonly nint _windowHandle;
        public Win32UIElement(Func<Win32AutomationService> win32AutomationSerivceGetter, 
            nint windowHandle,
            string? additionText = null) 
        {
            _windowHandle = windowHandle;
            _win32AutomationSerivceGetter = win32AutomationSerivceGetter;
            AdditionText = additionText;
        }

        public string? ClassName
        {
            get
            {
                var win32Service = EnsureWin32AutomationService();
                return win32Service.GetWindowClassName(_windowHandle);
            }
        }

        public int? ProcessId
        {
            get
            {
                return -1;
            }
        }

        public string? ControlType => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsVisible
        {
            get
            {
                return RegionRectangle != Rectangle.Empty;
            }
        }

        public string Name
        {
            get
            {
                var win32Service = EnsureWin32AutomationService();
                return win32Service.GetWindowText(_windowHandle);
            }
        }

        public Rectangle RegionRectangle
        {
            get
            {
                var win32Service = EnsureWin32AutomationService();
                return win32Service.GetWindowRectangle(_windowHandle);
            }
        }

        public int? ElementHandle
        {
            get
            {
                return _windowHandle.ToInt32();
            }
        }

        public string? WindowTitle
        {
            get
            {
                return Name;
            }
        }

        public int? RootHandle => throw new NotImplementedException();

        public string? AdditionText
        {
            get;
        }

        public string FrameworkType
        {
            get;
        } = "none";
        public IEnumerable<IUIElement> FindAllChildren()
        {
            //return _flauiElement.FindAllChildren().Select(x => new FlaUI3Element(x));
            return Enumerable.Empty<IUIElement>();
        }

        private Win32AutomationService EnsureWin32AutomationService()
        {
            _win32AutomationSerivce ??= _win32AutomationSerivceGetter();
            return _win32AutomationSerivce;
        }
    }
}
