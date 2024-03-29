using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;
using Application = System.Windows.Application;

namespace Lemon.Automation.Bootstrapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IWpfApplication
    {
        public App()
        {
            InitializeComponent();
        }
        public AssemblyName AssemblyName { get; private set; }
        public string AppName => throw new NotImplementedException();
        public SynchronizationContext AppSynchronizationContext { get; }
        public T ResolveHostService<T>(IServiceProvider serviceProvider) where T : IHostedService
        {
            throw new NotImplementedException();
        }
        public void Run(string[] runArgs)
        {
            throw new NotImplementedException();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new();
            window.Show();
        }
    }

}
