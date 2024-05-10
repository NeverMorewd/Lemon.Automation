using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class ServiceRegistryBase
    {
        private ConcurrentDictionary<Type, object> m_services = new ConcurrentDictionary<Type, object>();

        protected ServiceRegistryBase()
        {
            ResetServicesImpl();
        }

        protected void ResetServicesImpl()
        {
            IEnvironmentProvider environmentProvider = new EnvironmentProvider();
            IRuntimeInfo runtimeInfo = new RuntimeInfo();
            IPlatformInformation platformInformation = new PlatformInformation(environmentProvider, runtimeInfo);
            IShellExecute service = new ShellExecute();
            INativeService nativeService2;
            //if (!platformInformation.IsWindows)
            //{
            //    //if (!platformInformation.IsOSX)
            //    //{
            //    //    INativeService nativeService = new NativeLinux();
            //    //    nativeService2 = nativeService;
            //    //}
            //    //else
            //    //{
            //    //    INativeService nativeService = new NativeOSX(environmentProvider);
            //    //    nativeService2 = nativeService;
            //    //}
            //}
            //else
            //{
            //    INativeService nativeService = new NativeWindows();
            //    nativeService2 = nativeService;
            //}
            INativeService nativeService = new NativeWindows();
            nativeService2 = nativeService;
            INativeService nativeService3 = nativeService2;
            IProcessService processService = new ProcessService(nativeService3);
            IBrowserContext service2 = new BrowserContext(platformInformation, processService);
            IBrowserBridgePipeService service3 = new BrowserBridgePipeService(platformInformation, nativeService3);
            RegisterImpl(environmentProvider);
            RegisterImpl(runtimeInfo);
            RegisterImpl(platformInformation);
            RegisterImpl(service);
            RegisterImpl(nativeService3);
            RegisterImpl(processService);
            RegisterImpl(service2);
            RegisterImpl(service3);
        }

        protected TService GetImpl<TService>() where TService : class
        {
            if (m_services.TryGetValue(typeof(TService), out var obj))
            {
                return obj as TService;
            }
            return null;
        }

        protected void RegisterImpl<TService>(TService service) where TService : class
        {
            m_services[typeof(TService)] = service;
        }
    }
}
