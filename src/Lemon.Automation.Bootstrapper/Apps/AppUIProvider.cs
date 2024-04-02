using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;

namespace Lemon.Automation.Bootstrapper.Apps
{
    public class AppUIProvider : ApplicationContext, IBackgroundApplication
    {
        private IAppHostedService? _service;
        private Assembly? _entryPointAssembly;
        private readonly ManualResetEventSlim _loadResetEvent;
        public AppUIProvider()
        {
            _loadResetEvent = new ManualResetEventSlim();
            AssemblyName = new AssemblyName("Lemon.Automation.UIProvider");
        }
        public AssemblyName AssemblyName { get; private set; }
        public string AppName => nameof(AppUIProvider);
        public SynchronizationContext AppSynchronizationContext 
        { 
            get 
            {
                return SynchronizationContext.Current;
            } 
        }
        public T ResolveHostService<T>(IServiceProvider serviceProvider) where T:IHostedService
        {
            _entryPointAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lemon.Automation.UIProvider.dll"));
            var instance = _entryPointAssembly.CreateInstance("Lemon.Automation.UIProvider.UIProviderService", 
                ignoreCase:false,
                BindingFlags.Public | BindingFlags.Instance, 
                binder: null, 
                args: [serviceProvider], 
                culture: null, 
                activationAttributes: null);
            if (instance is IAppHostedService service)
            {
                _service = service;
            }
            if (_service == null)
            {
                throw new TypeLoadException(nameof(IAppHostedService));
            }
            return (T)_service;
        }
        public void Run(string[] runArgs)
        {
            Application.Run(this);
        }
    }
}
