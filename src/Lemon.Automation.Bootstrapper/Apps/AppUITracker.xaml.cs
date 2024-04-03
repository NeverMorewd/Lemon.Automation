using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using Application = System.Windows.Application;

namespace Lemon.Automation.Bootstrapper
{
    /// <summary>
    /// Interaction logic for AppUITracker.xaml
    /// </summary>
    public partial class AppUITracker : Application, IWpfApplication
    {
        private Assembly? _entryPointAssembly;
        private IAppHostedService? _service;
        public AppUITracker()
        {
            InitializeComponent();
        }
        public AssemblyName AssemblyName { get; private set; }
        public string AppName => nameof(AppUITracker);
        public SynchronizationContext AppSynchronizationContext { get; }
        public T ResolveHostService<T>(IServiceProvider serviceProvider) where T : IHostedService
        {
            _entryPointAssembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lemon.Automation.UITracker.dll"));
            var instance = _entryPointAssembly.CreateInstance("Lemon.Automation.UITracker.UITrackerService",
                ignoreCase: false,
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
            Run();
        }
    }
}
