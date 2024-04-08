using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop.GrpcServices;
using Lemon.Automation.UIProvider.GrpcServers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lemon.Automation.UIProvider
{
    public class AppUIProvider : ApplicationContext, IWinformApplication
    {
        private readonly IAppHostedService? _service;
        private readonly IServiceCollection _serviceCollection;
        public AppUIProvider(IServiceCollection serviceCollection):base()
        {
            AssemblyName = new AssemblyName("Lemon.Automation.UIProvider");
            _serviceCollection = serviceCollection;
            _serviceCollection
                .AddSingleton<IGrpcServer, GrpcNamedPipeServer>()
                .AddSingleton<GrpcServerWorkShop>()
                .AddKeyedSingleton<IGrpcService,UIAutomationGrpcService>(nameof(IGrpcService))
                .AddKeyedSingleton<IGrpcService,BeepGrpcService>(nameof(IGrpcService))
                .AddSingleton(sp => sp.GetKeyedServices<IGrpcService>(nameof(IGrpcService)))
                .AddSingleton<IAppHostedService, UIProviderHostedService>();
        }
        public AssemblyName AssemblyName 
        { 
            get; 
            private set; 
        }
        public string AppName => nameof(AppUIProvider);
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
