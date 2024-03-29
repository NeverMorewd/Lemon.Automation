using Lemon.Automation.Bootstrapper.Apps;
using Lemon.Automation.Domains;
using Lemon.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Windows.Win32;

namespace Lemon.Automation.Bootstrapper
{
    internal class Program
    {
        //private static IApplication app;

        [STAThread]
        static void Main(string[] args)
        {
            PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
            var _host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(AppContext.BaseDirectory)
                          .AddCommandLine(args)
                          .AddJsonFile("appsettings.json")
                          .Build();
                })
                .ConfigureServices((context, services) =>
                {
                    //services.AddHostedService<LongRunningTaskService>();
                    //services.AddHostedService<IHostedService>(o => new LongRunningTaskService());
                    //var configs = context.Configuration.GetChildren();
                    var app = AppFactory.ResolveDefaultApplication();
                    services.AddSingleton<IConnection,ConnectionService>()
                            .AddHostedService(sp => app.ResolveHostService<IUIProviderService>(sp))
                            .AddSingleton(app);
                })
                .ConfigureLogging(log =>
                {
                    log.Services.AddLogging(b => b.AddConsole());
                })
                .Build();

            _host.Start();
            var app = _host.Services.GetService<IApplication>();
            app.Run(args);
        }


    }
}
