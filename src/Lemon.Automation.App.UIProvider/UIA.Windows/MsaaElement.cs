using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    public class MsaaElement : IUIAElement
    {
        public string CacheId => throw new NotImplementedException();

        public string? ClassName => throw new NotImplementedException();

        public string? RootWindowClassName => throw new NotImplementedException();

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

        public string ProcessName => throw new NotImplementedException();

        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Tag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public JsonTextContent JsonContent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IProtobufSerializable? Deserialize(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUIAElement> FindAllChildren()
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
