using ProtoBuf;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lemon.Automation.Framework.Extensions
{
    /// <summary>
    /// https://stackoverflow.com/questions/13235279/protobuf-net-and-de-serializing-objects
    /// </summary>
    public static class ProtobufExtension
    {
        public static byte[] Serialize(this object thisObject)
        {
            using var ms = new MemoryStream();
            Type type = thisObject.GetType();
            var id = Encoding.ASCII.GetBytes(type.FullName + '|');
            ms.Write(id, 0, id.Length);
            Serializer.Serialize(ms, thisObject);
            var bytes = ms.ToArray();
            return bytes;
        }

        public static object Deserialize(this byte[] thisBytes)
        {
            StringBuilder sb = new();
            using var ms = new MemoryStream(thisBytes);
            while (true)
            {
                var currentChar = (char)ms.ReadByte();
                if (currentChar == '|')
                {
                    break;
                }

                sb.Append(currentChar);
            }

            string typeName = sb.ToString();

            // assuming that the calling assembly contains the desired type.
            // You can include aditional assembly information if necessary
            Type deserializationType = Assembly.GetCallingAssembly().GetType(typeName);

            MethodInfo mi = typeof(Serializer).GetMethod("Deserialize");
            MethodInfo genericMethod = mi.MakeGenericMethod([deserializationType]);
            return genericMethod.Invoke(null, new[] { ms });
        }
    }
}
