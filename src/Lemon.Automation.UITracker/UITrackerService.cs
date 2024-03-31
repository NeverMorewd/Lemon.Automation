using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lemon.Automation.UITracker
{
    public class UITrackerService : IUITrackerService
    {
        private readonly IServiceProvider _serviceProvider;
        public UITrackerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var app = _serviceProvider.GetService(typeof(IApplication)) as IApplication;
            //SynchronizationContext.
            Console.WriteLine($"CurrentThread:{Thread.CurrentThread.ManagedThreadId}");
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task HandleActivationAsync()
        {
            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                return Task.CompletedTask;
            }

            //Window mainWindow = _serviceProvider.GetRequiredService<IWindow>();
            //mainWindow.Loaded += OnMainWindowLoaded;
            //mainWindow?.Show();

            return Task.CompletedTask;
        }
    }
}
