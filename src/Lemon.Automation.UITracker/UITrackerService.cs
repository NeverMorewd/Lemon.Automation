using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Lemon.Automation.UITracker
{
    public class UITrackerService : IAppHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private SynchronizationContext _synchronizationContext;
        public UITrackerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var app = _serviceProvider.GetService(typeof(IApplication)) as IApplication;
            //SynchronizationContext.
            Console.WriteLine($"CurrentThread:{Environment.CurrentManagedThreadId}");
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
            MainWindow mainWindow = new();
            mainWindow.Loaded += MainWindow_Loaded;
            mainWindow.Closed += MainWindow_Closed;
            mainWindow.Show();

            return Task.CompletedTask;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            StopAsync(CancellationToken.None);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _synchronizationContext = new DispatcherSynchronizationContext(((Window)sender).Dispatcher);
        }
    }
}
