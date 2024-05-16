﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Lemon.Automation.BrowserNativeHost
{
    /// <summary>
    /// https://stackoverflow.com/questions/30880709/c-sharp-native-host-with-chrome-native-messaging
    /// </summary>
    public static class SimpleNativeMessagingHost
    {
        public static void Run(string[] args)
        {
            JObject data;
            while (true)
            {
                data = Read();
                if (data != null)
                {
                    var processed = ProcessMessage(data);
                    Write(processed);
                    if (processed == "exit")
                    {
                        return;
                    }
                }
            }
        }

        public static string ProcessMessage(JObject data)
        {
            var message = data["text"].Value<string>();
            switch (message)
            {
                case "test":
                    return "testing!";
                case "exit":
                    return "exit";
                default:
                    return "echo: " + message;
            }
        }

        public static JObject Read()
        {
            var stdin = Console.OpenStandardInput();
            var length = 0;

            var lengthBytes = new byte[4];
            stdin.Read(lengthBytes, 0, 4);
            length = BitConverter.ToInt32(lengthBytes, 0);

            var buffer = new char[length];
            using (var reader = new StreamReader(stdin))
            {
                while (reader.Peek() >= 0)
                {
                    reader.Read(buffer, 0, buffer.Length);
                }
            }
            var readString = new string(buffer);
            Console.WriteLine(readString);
            return JsonConvert.DeserializeObject<JObject>(readString);
        }

        public static void Write(JToken data)
        {
            var json = new JObject();
            json["data"] = data;

            var bytes = System.Text.Encoding.UTF8.GetBytes(json.ToString(Formatting.None));

            var stdout = Console.OpenStandardOutput();
            stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }
    }
}
