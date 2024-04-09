using Lemon.Automation.App.UIProvider.GrpcServers;
using Lemon.Automation.Domains;
using Lemon.Automation.Globals;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop.GrpcServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Lemon.Automation.App.UIProvider
{
    public class App : ApplicationContext, IWinformApplication
    {
        private readonly IAppHostedService? _service;
        private readonly IServiceCollection _serviceCollection;
        private readonly ILogger _logger;
        private readonly SynchronizationContext? _synchronizationContext;
        public App(IServiceCollection serviceCollection):base()
        {
            _serviceCollection = serviceCollection;
            if (SynchronizationContext.Current == null)
            {
                //https://github.com/dotnet/runtime/issues/94252
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                _synchronizationContext = SynchronizationContext.Current;
                if (_synchronizationContext != null)
                {
                    _serviceCollection.AddSingleton(_synchronizationContext);
                }
            }
            _logger = _serviceCollection.BuildServiceProvider().GetRequiredService<ILogger<App>>();

            _serviceCollection
                .AddSingleton<IGrpcServer, GrpcNamedPipeServer>()
                .AddSingleton<GrpcServerWorkShop>()
                .AddKeyedSingleton<IGrpcService,UIAutomationGrpcService>(nameof(IGrpcService))
                .AddKeyedSingleton<IGrpcService,BeepGrpcService>(nameof(IGrpcService))
                .AddSingleton(sp => sp.GetKeyedServices<IGrpcService>(nameof(IGrpcService)))
                .AddSingleton<IAppHostedService, HostedService>();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            Application.ThreadExit += Application_ThreadExit;
            Application.ApplicationExit += Application_ApplicationExit;
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            _logger.LogInformation("Application_Idle");
        }

        private void Application_ApplicationExit(object? sender, EventArgs e)
        {
            
        }

        private void Application_ThreadExit(object? sender, EventArgs e)
        {
            
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _logger.LogError(AppLogEventIds.Global, e.Exception, "Application_ThreadException");
        }

        public AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();
        public string? AppName => AssemblyName.Name;
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
