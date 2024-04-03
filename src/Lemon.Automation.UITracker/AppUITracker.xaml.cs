using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Lemon.Automation.UITracker
{
    /// <summary>
    /// Interaction logic for AppUITracker.xaml
    /// </summary>
    public partial class AppUITracker : Application, IWpfApplication
    {
        public AppUITracker()
        {
            InitializeComponent();
        }
        public AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();
        public string AppName => nameof(AppUITracker);
        public SynchronizationContext AppSynchronizationContext => new DispatcherSynchronizationContext(Current.Dispatcher);
        
        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            return new UITrackerService(serviceProvider);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        public void Run(string[] runArgs)
        {
            Run();
        }
    }
}
