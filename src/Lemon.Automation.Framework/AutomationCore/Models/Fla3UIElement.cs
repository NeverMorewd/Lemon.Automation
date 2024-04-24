using FlaUI.Core.AutomationElements;
using Lemon.Automation.Framework.AutomationCore.Domains;
using ProtoBuf;
using System.Drawing;
using System.IO;

namespace Lemon.Automation.Framework.AutomationCore.Models
{
    [ProtoContract]
    public class FlaUI3Element : IUIElement
    {
        private readonly AutomationElement _flauiElement;
        public FlaUI3Element()
        {
            
        }
        public FlaUI3Element(AutomationElement flauiElement, string? additionText = null)
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
        [ProtoMember(1)]
        public string? ClassName
        {
            get;
        }
        [ProtoMember(2)]
        public int? ProcessId
        {
            get;
        }
        [ProtoMember(3)]
        public string? ControlType
        {
            get;
        }
        [ProtoMember(4)]
        public bool IsAvailable
        {
            get;
            private set;
        }
        [ProtoMember(5)]
        public bool IsVisible
        {
            get;
            private set;
        }
        [ProtoMember(6)]
        public string? Name
        {
            get;
            private set;
        }
        [ProtoIgnore]
        public Rectangle RegionRectangle
        {
            get;
            private set;
        }
        [ProtoIgnore]
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
        [ProtoIgnore]
        public string? WindowTitle
        {
            get
            {
                //todo
                return string.Empty;
            }
        }
        [ProtoIgnore]
        public int? RootHandle
        {
            get
            {
                //todo
                return nint.Zero.ToInt32();
            }
        }
        [ProtoMember(12)]
        public string? AdditionText
        {
            get;
        }
        [ProtoMember(11)]
        public string FrameworkType
        {
            get;
        } = "none";
        [ProtoIgnore]
        public AutomationElement FlauiElement => _flauiElement;

        public IEnumerable<IUIElement> FindAllChildren()
        {
            return _flauiElement.FindAllChildren().Select(x => new FlaUI3Element(x));
        }

        public static byte[] Serialize(FlaUI3Element target)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, target);
            return stream.ToArray();
        }

        public static FlaUI3Element? Deserialize(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            return Serializer.Deserialize<FlaUI3Element>(stream);
        }
    }
}
