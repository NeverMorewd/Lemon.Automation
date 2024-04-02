using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop.GrpcServices;
using System.Windows.Threading;

namespace Lemon.Automation.UIProvider
{
    public class UIProviderService : IAppHostedService
    {
        private readonly GrpcNamedPipeServer _server;
        private readonly GrpcServerWorkShop _serverWorkShop;
        private readonly IEnumerable<IGrpcService> _grpcServices;
        private readonly IServiceProvider _serviceProvider;
        //https://github.com/dotnet/runtime/issues/94252
        private readonly SynchronizationContext? _synchronizationContext;
        public UIProviderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _server = new GrpcNamedPipeServer("1029384756");
            _grpcServices = new List<IGrpcService>
            {
                new UIAutomationGrpcService()
            };
            _serverWorkShop = new GrpcServerWorkShop(_grpcServices, _server);  
            Console.WriteLine($"CurrentThread:{Thread.CurrentThread.ManagedThreadId}");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                _serverWorkShop.Process();
                _server.Start();
                Console.WriteLine("UIProviderHostService started");
            }, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _server.Stop();
            await Task.CompletedTask;
        }
    }
}
