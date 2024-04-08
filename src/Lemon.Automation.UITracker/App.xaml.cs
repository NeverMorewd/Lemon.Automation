using Lemon.Automation.Domains;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Lemon.Automation.App.UITracker
{
    /// <summary>
    /// Interaction logic for AppUITracker.xaml
    /// </summary>
    public partial class App : Application, IWpfApplication
    {
        private readonly IServiceCollection _serviceCollection;
        public App(IServiceCollection serviceCollection):base()
        {
            InitializeComponent();
            _serviceCollection = serviceCollection;
            _serviceCollection
                .AddSingleton<IAppHostedService, HostedService>();
        }
        public AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();
        public string AppName => AssemblyName.Name;
        public SynchronizationContext AppSynchronizationContext => new DispatcherSynchronizationContext(Current.Dispatcher);
        
        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IAppHostedService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        public void Run(string[]? runArgs)
        {
            Run();
        }
    }
}
