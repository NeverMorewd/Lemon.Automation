using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lemon.Automation.Bootstrapper.Apps
{
    public class AppUIProvider : ApplicationContext, IBackgroundApplication
    {
        private IAppHostedService? _service;
        private Assembly _entryPointAssembly;
        private SynchronizationContext _context;
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
                if (_loadResetEvent.Wait(2000))
                {
                    return _context;
                }
                return null;
            } 
        }
        public T ResolveHostService<T>(IServiceProvider serviceProvider) where T:IHostedService
        {
            _entryPointAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lemon.Automation.UIProvider.dll"));
            var instance = _entryPointAssembly.CreateInstance("Lemon.Automation.UIProvider.UIProviderHostService", 
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
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            _context = SynchronizationContext.Current;
            _loadResetEvent.Set();
            Application.Run(this);
        }
    }
}
