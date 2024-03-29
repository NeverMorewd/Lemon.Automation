using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper.Apps
{
    internal class AppFactory
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IWpfApplication ResolveApplication()
        {
            return ResolveAppUITracker();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IWpfApplication ResolveAppStudio()
        {
            Console.WriteLine("RunAppStudio");
            return new AppStudio();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IWpfApplication ResolveAppUITracker()
        {
            Console.WriteLine("RunAppUITracker");
            return new AppUITracker();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IWpfApplication ResolveAppDefault()
        {
            Console.WriteLine("RunAppDefault");
            return new App();
        }
    }
}
