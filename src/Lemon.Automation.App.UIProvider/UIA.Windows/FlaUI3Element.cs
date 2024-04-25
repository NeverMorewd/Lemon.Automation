using FlaUI.Core.AutomationElements;
using Lemon.Automation.Domains;
using ProtoBuf;
using System.IO;

namespace Lemon.Automation.Framework.AutomationCore.Models
{
    public class Flaui3Element : IUIAElement
    {
        private readonly AutomationElement _flauiElement;
        public Flaui3Element(AutomationElement flauiElement, string? additionText = null)
        {
            _flauiElement = flauiElement;
            IsVisible = false;
            if (_flauiElement.Properties.BoundingRectangle.IsSupported)
            {
                RegionRectangle = _flauiElement.Properties.BoundingRectangle.ValueOrDefault;
                if (RegionRectangle != default)
                {
                    if (RegionRectangle.Height > 0 && RegionRectangle.Height > 0)
                    {
                        IsVisible = true;
                    }
                }
            }

            if (_flauiElement.Properties.Name.IsSupported)
            {
                Name = _flauiElement.Properties.Name.ValueOrDefault;
            }

            if (_flauiElement.Properties.ControlType.IsSupported)
            {
                ControlType = _flauiElement.Properties.ControlType.ValueOrDefault.ToString();
            }

            if (_flauiElement.Properties.ProcessId.IsSupported)
            {
                ProcessId = _flauiElement.Properties.ProcessId.ValueOrDefault;
            }

            if (_flauiElement.Properties.ClassName.IsSupported)
            {
                ClassName = _flauiElement.Properties.ClassName.ValueOrDefault;
            }

            FrameworkType = _flauiElement.FrameworkType.ToString();

            IsAvailable = _flauiElement.IsAvailable;
            AdditionText = additionText;
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
            private set;
        }
        public bool IsVisible
        {
            get;
            private set;
        }
        public string? Name
        {
            get;
            private set;
        }
        public Rectangle RegionRectangle
        {
            get;
            private set;
        }
        public int? ElementHandle
        {
            get
            {
                if (_flauiElement.Properties.NativeWindowHandle.IsSupported)
                {
                    return _flauiElement.Properties.NativeWindowHandle.ValueOrDefault.ToInt32();
                }
                return nint.Zero.ToInt32();
            }
        }
        public string? WindowTitle
        {
            get
            {
                //todo
                return string.Empty;
            }
        }
        public nint RootHandle
        {
            get
            {
                //todo
                return nint.Zero.ToInt32();
            }
        }
        public string? AdditionText
        {
            get;
        }
        public string FrameworkType
        {
            get;
        } = "none";
        public AutomationElement FlauiElement => _flauiElement;

        public string CacheId => throw new NotImplementedException();

        public string ProcessName => throw new NotImplementedException();

        public IEnumerable<IUIAElement> FindAllChildren()
        {
            return _flauiElement.FindAllChildren().Select(x => new Flaui3Element(x));
        }

        public static byte[] Serialize(Flaui3Element target)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, target);
            return stream.ToArray();
        }

        public static Flaui3Element? Deserialize(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            return Serializer.Deserialize<Flaui3Element>(stream);
        }
    }
}
