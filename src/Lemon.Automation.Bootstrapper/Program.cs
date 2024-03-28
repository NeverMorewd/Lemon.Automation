using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lemon.Automation.Bootstrapper
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        [STAThread]
        static void Main(string[] args)
        {

            AttachConsole(ATTACH_PARENT_PROCESS);
            if (args != null && args.Any())
            {
                Console.WriteLine(args);
                //RunApp();
            }
            else
            {
                RunAppDefault();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void RunAppStudio()
        {
            Console.WriteLine("RunAppStudio");
            var appStudio = new AppStudio();
            appStudio.Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void RunAppUITracker()
        {
            Console.WriteLine("RunAppUITracker");
            var appUITracker = new AppUITracker();
            appUITracker.Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void RunAppDefault()
        {
            Console.WriteLine("RunAppDefault");
            var app = new App();
            app.Run();
        }
    }
}
