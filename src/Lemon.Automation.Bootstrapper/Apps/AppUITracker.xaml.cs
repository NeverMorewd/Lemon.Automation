using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
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
        public AppUITracker()
        {
            InitializeComponent();
        }
        public AssemblyName AssemblyName { get; private set; }
        public string AppName => nameof(AppUITracker);
        public SynchronizationContext AppSynchronizationContext { get; }
        public T ResolveHostService<T>(IServiceProvider serviceProvider) where T : IHostedService
        {
            throw new NotImplementedException();
        }
        public void Run(string[] runArgs)
        {
            Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new()
            {
                Title = AppName
            };
            window.Show();
        }
    }
}
