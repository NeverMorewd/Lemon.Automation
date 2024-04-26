using FlaUI.Core;
using FlaUI.UIA3;
using Lemon.Automation.App.UIProvider.GrpcServers;
using Lemon.Automation.App.UIProvider.UIA.Chrome;
using Lemon.Automation.App.UIProvider.UIA.Services;
using Lemon.Automation.App.UIProvider.UIA.Windows;
using Lemon.Automation.Domains;
using Lemon.Automation.Framework.AutomationCore.Models;
using Lemon.Automation.Globals;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.GrpcWorkShop.GrpcServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;
using System.Reflection;
using Application = System.Windows.Forms.Application;

namespace Lemon.Automation.App.UIProvider
{
    public class App : ApplicationContext, IWinformApplication
    {
        private readonly IAppHostedService? _service;
        private readonly IServiceCollection _serviceCollection;
        private readonly ILogger _logger;
        private readonly SynchronizationContext? _synchronizationContext;
        public App(IServiceCollection serviceCollection):base()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            Application.ThreadExit += Application_ThreadExit;
            Application.ApplicationExit += Application_ApplicationExit;
            Application.Idle += Application_Idle;
            _serviceCollection = serviceCollection;
            if (SynchronizationContext.Current == null)
            {
                //https://github.com/dotnet/runtime/issues/94252
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                _synchronizationContext = SynchronizationContext.Current;
                if (_synchronizationContext != null)
                {
                    _serviceCollection.AddSingleton(_synchronizationContext);
                }
            }
            _logger = _serviceCollection.BuildServiceProvider().GetRequiredService<ILogger<App>>();

            _ = _serviceCollection
                .AddSingleton<IGrpcServer, GrpcNamedPipeServer>()
                .AddSingleton<GrpcServerWorkShop>()
                .AddSingleton<AutomationBase, UIA3Automation>()
                .AddSingleton<Win32AutomationService>()
                .AddSingleton<WindowsInputService>()
                .AddSingleton(typeof(IElementCacheService<Win32Element>),typeof(Win32ElementCache))
                .AddSingleton(typeof(IElementCacheService<Flaui3Element>), typeof(Flaui3ElementCache))
                .AddKeyedSingleton<IUIATracker, UIAWindowsServiceFacade>(nameof(IUIATracker))
                .AddKeyedSingleton<IUIATracker, UIAChromeServiceFacade>(nameof(IUIATracker))
                .AddKeyedSingleton<IUIAServiceFacade, UIAWindowsServiceFacade>(nameof(IUIAServiceFacade))
                .AddKeyedSingleton<IUIAServiceFacade, UIAChromeServiceFacade>(nameof(IUIAServiceFacade))
                .AddSingleton<MSAAService>()
                .AddSingleton<IUIATrackService, UIATrackService>()
                .AddKeyedSingleton<IGrpcService, UIAutomationTrackGrpcService>(nameof(IGrpcService))
                .AddKeyedSingleton<IGrpcService, UIAutomationOperationGrpcService>(nameof(IGrpcService))
                .AddKeyedSingleton<IGrpcService, BeepGrpcService>(nameof(IGrpcService))
                .AddSingleton(sp => sp.GetKeyedServices<IGrpcService>(nameof(IGrpcService)))
                .AddSingleton(sp => sp.GetKeyedServices<IUIATracker>(nameof(IUIATracker)))
                .AddSingleton(sp => sp.GetKeyedServices<IUIAServiceFacade>(nameof(IUIAServiceFacade)))
                .AddSingleton<IAppHostedService, HostedService>();


        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            Application.Idle -= Application_Idle;
            _logger.LogInformation("Application_Idle");
        }

        private void Application_ApplicationExit(object? sender, EventArgs e)
        {
            
        }

        private void Application_ThreadExit(object? sender, EventArgs e)
        {
            
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _logger.LogError(AppLogEventIds.Global, e.Exception, "Application_ThreadException");
        }

        public AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();
        public string? AppName => AssemblyName.Name;
        public SynchronizationContext? AppSynchronizationContext 
        { 
            get; 
            private set; 
        }

        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IAppHostedService>();
        }
        public void Run(string[]? runArgs)
        {
            //https://github.com/Cysharp/R3?tab=readme-ov-file#wpf
            WpfProviderInitializer.SetDefaultObservableSystem(ex => Console.WriteLine($"R3 UnhandledException:{ex}"));
            //WinFormsProviderInitializer.SetDefaultObservableSystem(ex => Console.WriteLine($"R3 UnhandledException:{ex}"));
            Application.Run(this);
        }
    }
}
