using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop;
using Microsoft.Extensions.Logging;

namespace Lemon.Automation.App.UIProvider
{
    public class HostedService : IAppHostedService
    {
        private readonly GrpcServerWorkShop _serverWorkShop;
        private readonly IApplication _application;
        private readonly ILogger _logger;
        private readonly IConnection _connection;

        public HostedService(IApplication application,
            GrpcServerWorkShop grpcServerWorkShop,
            IConnection connection,
            ILogger<HostedService> logger)
        {
            _logger = logger;
            _serverWorkShop = grpcServerWorkShop;
            _application = application;
            _connection = connection;
            _logger.LogDebug($"{nameof(HostedService)} init thread:{Environment.CurrentManagedThreadId}");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Form form = new()
            //{
            //    Text = _application.AppName
            //};
            //form.FormClosed += (sender, args) => 
            //{
            //    StopAsync(cancellationToken);
            //    Environment.Exit(0);
            //};
            //form.Show();

            _serverWorkShop.Start();
            _logger.LogInformation($"{nameof(HostedService)} StartAsync");
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
