using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class Service : ServiceRegistryBase
    {
        private static Lazy<Service> s_instance = new Lazy<Service>(() => new Service());

        private Service()
        {
            IPlatformInformation impl = GetImpl<IPlatformInformation>();
            if (impl.IsWindows)
            {
                Win32Registry service = new Win32Registry();
                RegisterImpl((IWin32Registry)service);
            }
            //Log.EnabledLevel = LogLevel.Verbose;
            //Log.RegisterWriter(new TraceWriter());
        }

        public static TService Get<TService>() where TService : class
        {
            return s_instance.Value.GetImpl<TService>();
        }

        public static void Register<TService>(TService service) where TService : class
        {
            s_instance.Value.RegisterImpl(service);
        }
    }
}
