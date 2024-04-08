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
        public HostedService(IApplication application, ILogger<HostedService> logger)
        {
            _application = application;
            _logger = logger;
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
            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                return Task.CompletedTask;
            }
            //_synchronizationContext?.Send(o => 
            //{
            //    MainWindow mainWindow = new();
            //    mainWindow.Loaded += MainWindow_Loaded;
            //    mainWindow.Closed += MainWindow_Closed;
            //    mainWindow.Show();
            //}, null);
            MainWindow mainWindow = new();
            mainWindow.Title = _application.AppName;
            mainWindow.Loaded += MainWindow_Loaded;
            mainWindow.Closed += MainWindow_Closed;
            mainWindow.Show();
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
