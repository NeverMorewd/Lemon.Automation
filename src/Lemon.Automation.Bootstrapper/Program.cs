using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // TODO Whatever you want to do before starting
            // the WPF application and loading all WPF dlls
            RunApp();
        }

        // Ensure the method is not inlined, so you don't
        // need to load any WPF dll in the Main method
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void RunApp()
        {
            //var app = new App();
            //app.Run();

            //var appStudio = new AppStudio();
            //appStudio.Run();

            var appUITracker = new AppUITracker();
            appUITracker.Run();
        }
    }
}
