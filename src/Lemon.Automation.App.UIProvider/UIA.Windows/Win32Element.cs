using Lemon.Automation.Domains;
using ProtoBuf;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    public class Win32Element : IUIAElement
    {
        private readonly Func<Win32AutomationService> _win32AutomationSerivceGetter;
        private Win32AutomationService? _win32AutomationSerivce;
        private readonly nint _windowHandle;
        public Win32Element(Func<Win32AutomationService> win32AutomationSerivceGetter, 
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

        public nint RootHandle => throw new NotImplementedException();

        public string? AdditionText
        {
            get;
        }

        public string FrameworkType
        {
            get;
        } = "none";

        public string CacheId => throw new NotImplementedException();

        public string ProcessName => throw new NotImplementedException();

        public string? RootWindowClassName => throw new NotImplementedException();

        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string Tag { get; set; }
        [ProtoMember(3)]
        public JsonTextContent JsonContent { get; set; }

        public IEnumerable<IUIAElement> FindAllChildren()
        {
            //return _flauiElement.FindAllChildren().Select(x => new FlaUI3Element(x));
            return Enumerable.Empty<IUIAElement>();
        }

        private Win32AutomationService EnsureWin32AutomationService()
        {
            _win32AutomationSerivce ??= _win32AutomationSerivceGetter();
            return _win32AutomationSerivce;
        }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public IProtobufSerializable? Deserialize(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
