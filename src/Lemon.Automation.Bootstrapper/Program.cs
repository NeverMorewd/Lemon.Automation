using CommandLine;
using Lemon.Automation.Bootstrapper.Apps;
using Lemon.Automation.Bootstrapper.CommandLines;
using Lemon.Automation.Domains;
using Lemon.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using Windows.Win32;

namespace Lemon.Automation.Bootstrapper
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
            string? appName = null;
            Parser.Default
                  .ParseArguments<Options>(args)
                  .WithParsed(o =>
                  {
                      if (o.IsBootstrapper == true)
                      {
                          if (o.Apps.Any())
                          {
                              foreach (var app in o.Apps)
                              {
                                  var exeFilePath = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
                                  ProcessStartInfo startInfo = new()
                                  {
                                      FileName = exeFilePath,
                                      UseShellExecute = false,
                                      Arguments = $"-a {app} -b {false}"
                                  };
                                  //Process? process = Process.Start(startInfo);
                              }
                          }
                          Environment.Exit(0);
                      }
                      else
                      {
                          appName = o.Apps.First();
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
                                var app = AppFactory.ResolveApplication(appName);
                                services.AddSingleton<IConnection, ConnectionService>()
                                        .AddHostedService(sp => app.ResolveHostService<IAppHostedService>(sp))
                                        .AddSingleton(app);
                            })
                            .ConfigureLogging(log =>
                            {
                                log.Services.AddLogging(b => b.AddConsole());
                            })
                            .Build();

                            _host.Start();
                            var app = _host.Services.GetService<IApplication>();
                            app?.Run(args);
                      }
                  });
            
        }


    }
}
