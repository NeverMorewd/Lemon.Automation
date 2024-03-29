using Lemon.Automation.Bootstrapper.Apps;
using Lemon.Automation.Domains;
using System.Runtime.CompilerServices;
using Windows.Win32;

namespace Lemon.Automation.Bootstrapper
{
    internal class Program
    {
        private static IWpfApplication app;
        [STAThread]
        static void Main(string[] args)
        {
            PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
            app = AppFactory.ResolveApplication();
            app.Run(args);
        }

        
    }
}
