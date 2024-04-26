using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Domains
{
    [ProtoContract]
    [ProtoInclude(50, typeof(IUIAElement))]
    public interface IProtobufSerializable
    {
        [ProtoMember(1)]
        public string Id
        {
            get;
            set;
        }
        [ProtoMember(2)]
        public string Tag
        {
            get;
            set;
        }
        [ProtoMember(3)]
        public JsonTextContent JsonContent
        {
            get;
            set;
        }

        public byte[] Serialize();
        public IProtobufSerializable? Deserialize(byte[] bytes);
    }

    [ProtoContract]
    public class JsonTextContent
    {
        [ProtoMember(1)]
        public string TypeString
        {
            get;
            set;
        }
        [ProtoMember(2)]
        public string JsonText
        {
            get;
            set;
        }
    }
}
