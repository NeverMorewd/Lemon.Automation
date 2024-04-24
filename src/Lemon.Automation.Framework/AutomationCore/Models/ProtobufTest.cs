using MessagePack;
using ProtoBuf;
using System.IO;

namespace Lemon.Automation.Framework.AutomationCore.Models
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [MessagePackObject]
    public class ProtobufTest
    {
        [ProtoMember(1)]
        [Key(1)]
        public int Int32Value
        {
            get;
            set;
        }
        [ProtoMember(2)]
        [Key(1)]
        public string StringValue
        {
            get;
            set;
        } = string.Empty;
        [ProtoMember(3)]
        [Key(1)]
        public bool BoolValue
        {
            get;
            set;
        }
        [ProtoMember(4)]
        [Key(1)]
        public double DoubleValue
        {
            get;
            set;
        }

        public static ProtobufTest Create() 
        {
            return new ProtobufTest
            {
                Int32Value = new Random().Next(1, 9999),
                StringValue = "abc",
                BoolValue = true,
                DoubleValue = new Random().NextDouble(),
            };
        }

        public byte[] Serialize()
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, this);
            return stream.ToArray();
        }

        public static byte[] Serialize(ProtobufTest target)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, target);
            return stream.ToArray();
        }

        public static ProtobufTest? Deserialize(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            return Serializer.Deserialize<ProtobufTest>(stream);
        }
    }
}
