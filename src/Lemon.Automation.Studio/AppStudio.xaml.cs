using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = System.Windows.Application;

namespace Lemon.Automation.Studio
{
    /// <summary>
    /// Interaction logic for AppStudio.xaml
    /// </summary>
    public partial class AppStudio : Application, IWpfApplication
    {
        public AppStudio()
        {
            InitializeComponent();
        }
        public SynchronizationContext AppSynchronizationContext { get; }
        public AssemblyName AssemblyName { get; private set; }
        public string AppName => nameof(AppStudio);
        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
        public void Run(string[] runArgs)
        {
            Run();
        }
    }
}
