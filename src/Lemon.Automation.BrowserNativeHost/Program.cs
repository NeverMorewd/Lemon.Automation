using System.IO;
using System.Windows;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start...");
            MessageBox.Show("start");
            SimpleNativeMessagingHost.Run(args);

            //using Stream streamInput = Console.OpenStandardInput();
            //using Stream streamOutput = Console.OpenStandardOutput();
            //RunNativeHostLoop(streamInput, streamOutput);
        }

        public static void RunNativeHostLoop(Stream nativeHostInputStream, Stream nativeHostOutputStream)
        {
            using BrowserExtensionConnector browserExtensionConnector = new(nativeHostInputStream, nativeHostOutputStream);
            using NativeHostServer nativeHostServer = new(browserExtensionConnector);
            browserExtensionConnector.RunExtensionLoop();
            //Log.Info("Finished", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\ProgramMain.cs", 35, "RunNativeHostLoop");
        }
    }
}
