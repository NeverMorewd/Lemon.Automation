using Lemon.Automation.Commons;
using Lemon.Automation.Domains;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Lemon.Automation.Bootstrapper
{
    internal static class AppFactory
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IApplication ResolveApplication(string anAppName, Dictionary<string,AppSetting> anAppSettings)
        {
            if (anAppSettings != null && anAppSettings.Count > 0)
            {
                if (anAppSettings.TryGetValue(anAppName, out AppSetting? outAppSetting))
                {
                    if (outAppSetting != null)
                    {
                        return BuildApp(outAppSetting.AssemblyName, outAppSetting.TypeName);
                    }
                }
            }
            throw new InvalidOperationException();
            //return appName switch
            //{
            //    "AppStudio" => ResolveAppStudio(),
            //    "AppUITracker" => ResolveAppUITracker(),
            //    "AppUIProvider" => ResolveAppUIProvider(),
            //    "AppExecutor" => ResolveAppExecutor(),
            //    "AppDefault" => ResolveAppDefault(),
            //    _ => ResolveAppDefault(),
            //};
        }
        /// <summary>
        /// ResolveAppStudio
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IWpfApplication ResolveAppStudio()
        {
            Console.WriteLine("ResolveAppStudio");
            throw new NotImplementedException();
        }
        /// <summary>
        /// ResolveAppUITracker
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IWpfApplication ResolveAppUITracker()
        {
            Console.WriteLine("ResolveAppUITracker");
            var _entryPointAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lemon.Automation.UITracker.dll"));
            var appInstance = _entryPointAssembly.CreateInstance("Lemon.Automation.UITracker.AppUITracker");
            if (appInstance is IWpfApplication wpfApplication)
            {
                return wpfApplication;
            }
            throw new InvalidOperationException();
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// ResolveAppExecutor
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IApplication ResolveAppExecutor()
        {
            Console.WriteLine("ResolveAppExecutor");
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IApplication BuildApp(string anAssemblyName, string aTypeName)
        {
            var _entryPointAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, anAssemblyName));
            var appInstance = _entryPointAssembly.CreateInstance(aTypeName);
            if (appInstance is IWpfApplication wpfApplication)
            {
                return wpfApplication;
            }
            throw new InvalidOperationException();
        }
    }
}
