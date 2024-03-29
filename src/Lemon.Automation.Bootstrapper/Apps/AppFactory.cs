using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper.Apps
{
    internal static class AppFactory
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IApplication ResolveDefaultApplication()
        {
            return ResolveAppUIProvider();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IWpfApplication ResolveAppStudio()
        {
            Console.WriteLine("ResolveAppStudio");
            return new AppStudio();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IWpfApplication ResolveAppUITracker()
        {
            Console.WriteLine("ResolveAppUITracker");
            return new AppUITracker();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IWpfApplication ResolveAppDefault()
        {
            Console.WriteLine("ResolveAppDefault");
            return new App();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IApplication ResolveAppUIProvider()
        {
            Console.WriteLine("ResolveAppUIProvider");
            return new AppUIProvider();
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static IApplication ResolveAppExecutor()
        {
            Console.WriteLine("ResolveAppExecutor");
            return new AppExecutor();
        }
    }
}
