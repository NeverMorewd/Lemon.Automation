using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop.GrpcServices;
using Lemon.Automation.UIProvider.GrpcServers;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.UIProvider
{
    public class UIProviderHostedService : IAppHostedService
    {
        private readonly GrpcServerWorkShop _serverWorkShop;
        private readonly IApplication _application;
        private readonly ILogger _logger;
        //https://github.com/dotnet/runtime/issues/94252
        private readonly SynchronizationContext? _synchronizationContext;

        public UIProviderHostedService(IApplication application,
            GrpcServerWorkShop grpcServerWorkShop,
            ILogger<UIProviderHostedService> logger)
        {
            _logger = logger;
            _serverWorkShop = grpcServerWorkShop;
            _application = application;
            _logger.LogDebug($"{nameof(UIProviderHostedService)} init thread:{Environment.CurrentManagedThreadId}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Form form = new()
            {
                Text = _application.AppName
            };
            form.FormClosed += (sender, args) => 
            {
                StopAsync(cancellationToken);
                Environment.Exit(0);
            };
            form.Show();


            _serverWorkShop.Start();

            _application.Run(null);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _serverWorkShop.Stop();
            return Task.CompletedTask;
        }
    }
}
