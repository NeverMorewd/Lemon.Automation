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
        internal static IApplication ResolveApplication(string? appName)
        {
            return appName switch
            {
                "AppStudio" => ResolveAppStudio(),
                "AppUITracker" => ResolveAppUITracker(),
                "AppUIProvider" => ResolveAppUIProvider(),
                "AppExecutor" => ResolveAppExecutor(),
                "AppDefault" => ResolveAppDefault(),
                _ => ResolveAppDefault(),
            };
        }
        /// <summary>
        /// ResolveAppStudio
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IWpfApplication ResolveAppStudio()
        {
            Console.WriteLine("ResolveAppStudio");
            return new AppStudio();
        }
        /// <summary>
        /// ResolveAppUITracker
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IWpfApplication ResolveAppUITracker()
        {
            Console.WriteLine("ResolveAppUITracker");
            return new AppUITracker();
        }
        /// <summary>
        /// ResolveAppDefault
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IWpfApplication ResolveAppDefault()
        {
            Console.WriteLine("ResolveAppDefault");
            return new App();
        }
        /// <summary>
        /// ResolveAppUIProvider
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IApplication ResolveAppUIProvider()
        {
            Console.WriteLine("ResolveAppUIProvider");
            return new AppUIProvider();
        }
        /// <summary>
        /// ResolveAppExecutor
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IApplication ResolveAppExecutor()
        {
            Console.WriteLine("ResolveAppExecutor");
            return new AppExecutor();
        }
    }
}
