using CommunityToolkit.Mvvm.Messaging;
using Gma.System.MouseKeyHook;
using Lemon.Automation.App.UITracker.Services;
using Lemon.Automation.App.UITracker.ViewModels;
using Lemon.Automation.App.UITracker.Views;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcProvider.GrpcClients;
using Microsoft.Extensions.DependencyInjection;
using R3;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace Lemon.Automation.App.UITracker
{
    /// <summary>
    /// Interaction logic for AppUITracker.xaml
    /// </summary>
    public partial class App : Application, IWpfApplication
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IKeyboardMouseEvents _globalHook;
        public App(IServiceCollection serviceCollection) : base()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            InitializeComponent();
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            _globalHook = Hook.GlobalEvents();
            _serviceCollection = serviceCollection;

            _serviceCollection.AddSingleton<UIAutomationGrpcClientProvider>()
                              .AddSingleton<ElementHighlightService>()
                              .AddSingleton<ElementInspectService>()
                              .AddSingleton<TestViewMode>()
                              .AddSingleton<TrackViewModel>()
                              .AddSingleton<InspectViewModel>()
                              .AddSingleton<HomeViewModel>()
                              .AddSingleton<ElementTrackService>()
                              .AddSingleton<MainWindowViewModel>()
                              .AddSingleton<MainWindow>()
                              .AddSingleton(_globalHook)
                              .AddSingleton(WeakReferenceMessenger.Default)
                              .AddSingleton<IAppHostedService, HostedService>();

            Services = _serviceCollection.BuildServiceProvider();
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Console.WriteLine($"UnobservedTaskException: {e.Exception}");
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"DispatcherUnhandledException: {e.Exception}");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"UnhandledException: {e.ExceptionObject}");
        }

        public new static App Current => (App)Application.Current;
        public IServiceProvider Services
        {
            get;
        }

        public AssemblyName AssemblyName => Assembly.GetExecutingAssembly().GetName();
        public string? AppName => AssemblyName.Name;
        public SynchronizationContext AppSynchronizationContext => new DispatcherSynchronizationContext(Current.Dispatcher);

        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IAppHostedService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //https://github.com/Cysharp/R3?tab=readme-ov-file#wpf
            WpfProviderInitializer.SetDefaultObservableSystem(ex => Console.WriteLine($"R3 UnhandledException:{ex}"));
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
        public void Run(string[]? runArgs)
        {
            Run();
        }
    }
}
