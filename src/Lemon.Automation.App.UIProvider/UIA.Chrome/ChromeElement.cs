using Lemon.Automation.Domains;

namespace Lemon.Automation.Framework.AutomationCore.Models
{
    public class ChromeElement : IUIAElement
    {
        public string? ClassName => throw new NotImplementedException();

        public int? ProcessId => throw new NotImplementedException();

        public string? ControlType => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsVisible => throw new NotImplementedException();

        public string? Name => throw new NotImplementedException();

        public Rectangle RegionRectangle => throw new NotImplementedException();

        public int? ElementHandle => throw new NotImplementedException();

        public string? WindowTitle => throw new NotImplementedException();

        public nint RootHandle => throw new NotImplementedException();

        public string? AdditionText => throw new NotImplementedException();

        public string FrameworkType => throw new NotImplementedException();

        public string CacheId => throw new NotImplementedException();

        public string ProcessName => throw new NotImplementedException();

        public IEnumerable<IUIAElement> FindAllChildren()
        {
            throw new NotImplementedException();
        }
    }
}
