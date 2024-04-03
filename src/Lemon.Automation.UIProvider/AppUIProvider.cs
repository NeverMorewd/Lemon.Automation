using Lemon.Automation.Domains;
using System.Reflection;

namespace Lemon.Automation.UIProvider
{
    public class AppUIProvider : ApplicationContext, IWinformApplication
    {
        private IAppHostedService? _service;
        public AppUIProvider()
        {
            AssemblyName = new AssemblyName("Lemon.Automation.UIProvider");
        }
        public AssemblyName AssemblyName { get; private set; }
        public string AppName => nameof(AppUIProvider);
        public SynchronizationContext AppSynchronizationContext { get; private set; }
        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            return new UIProviderService(serviceProvider);
        }
        public void Run(string[] runArgs)
        {
            Application.Run(this);
        }
    }
}
