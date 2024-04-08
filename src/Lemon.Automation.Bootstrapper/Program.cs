﻿using CommandLine;
using Lemon.Automation.Bootstrapper.CommandLines;
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

namespace Lemon.Automation.Bootstrapper
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            PInvoke.AttachConsole(PInvoke.ATTACH_PARENT_PROCESS);
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
                          Environment.Exit(0);
                      }
                      else
                      {
                          appName = o.Apps.First();
                          var host = Host.CreateDefaultBuilder(args)
                            .ConfigureAppConfiguration(config =>
                            {
                                config.SetBasePath(AppContext.BaseDirectory)
                                        .AddCommandLine(args)
                                        .AddJsonFile("appsettings.json")
                                        .Build();
                            })
                            .ConfigureLogging(logging =>
                            {
                                //log.Services.AddLogging(b => b.AddConsole());
                                logging.ClearProviders();
                                logging.AddConsole();
                            })
                            .ConfigureServices((context, services) =>
                            {
                                var appsSection = context.Configuration.GetSection("Apps");
                                var appSettings = appsSection.Get<Dictionary<string, AppSetting>>();

                                if (appSettings != null)
                                {
                                    var app = AppFactory.ResolveApplication(appName, appSettings, services);
                                    services.AddSingleton<IConnection, ConnectionService>()
                                            .AddSingleton(app)
                                            .AddHostedService(sp => app.ResolveHostService(sp))
                                            .AddSingleton(app);
                                }
                            })
                            .Build();
                            host.Start();
                      }
                  });
            
        }
    }
}
