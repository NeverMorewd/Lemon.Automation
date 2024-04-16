using Lemon.Automation.Domains;
using Lemon.Automation.App.UITracker.Views;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Lemon.Automation.App.UITracker
{
    public class HostedService : IAppHostedService
    {
        private readonly IApplication _application;
        private readonly SynchronizationContext? _synchronizationContext;
        private readonly ILogger _logger;
        private readonly MainWindow _window;
        public HostedService(IApplication application, MainWindow window, ILogger<HostedService> logger)
        {
            _application = application;
            _logger = logger;
            _window = window;
            _synchronizationContext = _application.AppSynchronizationContext;
            _logger.LogDebug($"CurrentThread:{Environment.CurrentManagedThreadId}:{_synchronizationContext}");
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return HandleActivationAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task HandleActivationAsync()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            _window.Loaded += MainWindow_Loaded;
            _window.Closed += MainWindow_Closed;
            _window.Show();
            //if (Application.Current.Windows.OfType<MainWindow>().Any())
            //{
            //    return Task.CompletedTask;
            //}
            //_synchronizationContext?.Send(o => 
            //{
            //    MainWindow mainWindow = new();
            //    mainWindow.Loaded += MainWindow_Loaded;
            //    mainWindow.Closed += MainWindow_Closed;
            //    mainWindow.Show();
            //}, null);


            _application?.Run(null);
            return Task.CompletedTask;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            StopAsync(CancellationToken.None);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
