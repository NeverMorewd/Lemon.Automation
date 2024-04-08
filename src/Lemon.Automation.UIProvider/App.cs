using Lemon.Automation.App.UIProvider.GrpcServers;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop.GrpcServices;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lemon.Automation.App.UIProvider
{
    public class App : ApplicationContext, IWinformApplication
    {
        private readonly IAppHostedService? _service;
        private readonly IServiceCollection _serviceCollection;
        public App(IServiceCollection serviceCollection):base()
        {
            _serviceCollection = serviceCollection;
            _serviceCollection
                .AddSingleton<IGrpcServer, GrpcNamedPipeServer>()
                .AddSingleton<GrpcServerWorkShop>()
                .AddKeyedSingleton<IGrpcService,UIAutomationGrpcService>(nameof(IGrpcService))
                .AddKeyedSingleton<IGrpcService,BeepGrpcService>(nameof(IGrpcService))
                .AddSingleton(sp => sp.GetKeyedServices<IGrpcService>(nameof(IGrpcService)))
                .AddSingleton<IAppHostedService, HostedService>();
        }
        public AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();
        public string AppName => AssemblyName.Name;
        public SynchronizationContext? AppSynchronizationContext 
        { 
            get; 
            private set; 
        }

        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IAppHostedService>();
        }
        public void Run(string[]? runArgs)
        {
            Application.Run(this);
        }
    }
}
