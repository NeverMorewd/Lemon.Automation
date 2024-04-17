using Lemon.Automation.App.UITracker.Services;
using Lemon.Automation.App.UITracker.ViewModels;
using Lemon.Automation.App.UITracker.Views;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcProvider.GrpcClients;
using Microsoft.Extensions.DependencyInjection;
using R3;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
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
        public App(IServiceCollection serviceCollection) : base()
        {
            InitializeComponent();
            _serviceCollection = serviceCollection;
            _serviceCollection.AddSingleton<UIAutomationGrpcClientProvider>()
                              .AddTransient<ElementHighlightService>()
                              .AddSingleton<TestViewMode>()
                              .AddSingleton<TrackViewModel>()
                              .AddSingleton<InspectViewModel>()
                              .AddSingleton<HomeViewModel>()
                              .AddSingleton<ElementTrackService>()
                              .AddSingleton<MainWindowViewModel>()
                              .AddSingleton<MainWindow>()
                              .AddSingleton<IAppHostedService, HostedService>();

            Services = _serviceCollection.BuildServiceProvider();
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
            WpfProviderInitializer.SetDefaultObservableSystem(ex => Debug.WriteLine($"R3 UnhandledException:{ex}"));
        }

        public void Run(string[]? runArgs)
        {
            Run();
        }
    }
}
