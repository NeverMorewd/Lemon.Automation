using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Lemon.Automation.UITracker
{
    public class UITrackerService : IAppHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplication _application;
        private SynchronizationContext? _synchronizationContext;
        public UITrackerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _application = _serviceProvider.GetService(typeof(IApplication)) as IApplication;
            _synchronizationContext = _application?.AppSynchronizationContext;
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
            //_synchronizationContext?.Send(o => 
            //{
            //    MainWindow mainWindow = new();
            //    mainWindow.Loaded += MainWindow_Loaded;
            //    mainWindow.Closed += MainWindow_Closed;
            //    mainWindow.Show();
            //}, null);
            MainWindow mainWindow = new();
            mainWindow.Loaded += MainWindow_Loaded;
            mainWindow.Closed += MainWindow_Closed;
            mainWindow.Show();
            _application.Run(null);
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
