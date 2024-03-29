using Lemon.Automation.Domains;
using System.Windows;
using Application = System.Windows.Application;

namespace Lemon.Automation.Bootstrapper
{
    /// <summary>
    /// Interaction logic for AppUITracker.xaml
    /// </summary>
    public partial class AppUITracker : Application, IWpfApplication
    {
        public AppUITracker()
        {
            InitializeComponent();
        }
        public string AppName => nameof(AppUITracker);

        public void Run(string[] runArgs)
        {
            Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new()
            {
                Title = AppName
            };
            window.Show();
        }
    }
}
