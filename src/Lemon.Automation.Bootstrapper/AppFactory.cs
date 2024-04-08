using Lemon.Automation.Commons;
using Lemon.Automation.Domains;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Lemon.Automation.Bootstrapper
{
    internal static class AppFactory
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IApplication ResolveApplication(string anAppName, 
            Dictionary<string,AppSetting> anAppSettings,
            IServiceCollection aServiceCollection)
        {
            if (anAppSettings != null && anAppSettings.Count > 0)
            {
                if (anAppSettings.TryGetValue(anAppName, out AppSetting? outAppSetting))
                {
                    if (outAppSetting != null)
                    {
                        return BuildApp(outAppSetting.AssemblyName, 
                            outAppSetting.TypeName, 
                            aServiceCollection);
                    }
                }
            }
            throw new InvalidOperationException();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IApplication BuildApp(string anAssemblyName, 
            string aTypeName, 
            IServiceCollection aServiceCollection)
        {
            var _entryPointAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, anAssemblyName));
            var appInstance = _entryPointAssembly.CreateInstance(aTypeName, 
                false, 
                BindingFlags.Public | BindingFlags.Instance, 
                binder: null, 
                args: [aServiceCollection], 
                culture: null, 
                activationAttributes: null);

            if (appInstance is IApplication application)
            {
                return application;
            }
            throw new InvalidOperationException();
        }
    }
}
