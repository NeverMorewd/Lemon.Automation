using Lemon.Automation.Domains;
using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;

namespace Lemon.Automation.Bootstrapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IWpfApplication
    {
        public App()
        {
            InitializeComponent();
        }

        public string AppName => throw new NotImplementedException();

        public void Run(string[] runArgs)
        {
            throw new NotImplementedException();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new();
            window.Show();
        }
    }

}
