using CommandLine;
using Lemon.Automation.CommandLines;
using Lemon.Automation.Commons;
using Lemon.Automation.Domains;
using Lemon.Automation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using Windows.Win32;

namespace Lemon.Automation.App
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
            GlobalHandle();

            Parser.Default
                  .ParseArguments<CommandOptions>(args)
                  .WithParsed(options =>
                  {
                      Console.WriteLine(options);
                      RunWithArgs(options);
                  })
                  .WithNotParsed(errors => 
                  {
                      foreach (var error in errors)
                      {
                          Console.WriteLine($"CommandParam parse failed:{error.Tag},{error.StopsProcessing}");
                      }
                  });

            
        }

        static void RunWithArgs(CommandOptions options)
        {
            if (options.IsBootstrapper == true)
            {
                if (options.Apps.Any())
                {
                    string connectionKey = $"{Guid.NewGuid()}-{Environment.ProcessId}";
                    RunApps(options.Apps, connectionKey);
                }
                Environment.Exit(0);
            }
            else
            {
                if (options.Apps == null || !options.Apps.Any())
                {

                }
                else
                {
                    if (options.Apps.Count() > 1)
                    {
                        var apps = options.Apps.Skip(1);
                        RunApps(apps, options.ConnectionKey);
                    }
                    var appName = options.Apps.First();
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureAppConfiguration(config =>
                        {
                            config.SetBasePath(AppContext.BaseDirectory)
                                  .AddJsonFile("appsettings.json")
                                  .Build();
                        })
                        .ConfigureLogging(logging =>
                        {
                            logging.ClearProviders().AddConsole();
                        })
                        .ConfigureServices((context, services) =>
                        {
                            var appsSection = context.Configuration.GetSection("Apps");
                            var appSettings = appsSection.Get<Dictionary<string, AppSetting>>();

                            if (appSettings != null)
                            {
                                var app = AppFactory.ResolveApplication(appName, appSettings, services);
                                services.AddSingleton<IConnection, ConnectionService>()
                                        .AddSingleton(options)
                                        .AddSingleton(app)
                                        .AddHostedService(sp => app.ResolveHostService(sp));
                            }
                        })
                        .Build();
                    host.Start();
                }
            }
        }
        private static void RunApps(IEnumerable<string> apps, string? connectionKey)
        {
            var exeFilePath = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            foreach (var app in apps)
            {

                ProcessStartInfo startInfo = new()
                {
                    FileName = exeFilePath,
                    UseShellExecute = false,
                };
                if (string.IsNullOrEmpty(connectionKey))
                {
                    startInfo.Arguments = $"-a {app} -b {false}";
                }
                else
                {
                    startInfo.Arguments = $"-a {app} -b {false} -c {connectionKey}";
                }

                Process? process = Process.Start(startInfo);
                if (process != null)
                {
                    Console.WriteLine($"Start successfully:{process.Id}");
                }
                else
                {
                    Console.WriteLine($"Start failed!");
                }
            }
        }
        private static void GlobalHandle()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Console.WriteLine($"{e.Exception}");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                Console.WriteLine($"Fatal Error:{e.ExceptionObject}");
            }
            else
            {
                Console.WriteLine($"{e.ExceptionObject}");
            }
        }
    }
}
