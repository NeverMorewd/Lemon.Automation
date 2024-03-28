using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Lemon.Automation.Bootstrapper
{
    /// <summary>
    /// Interaction logic for AppStudio.xaml
    /// </summary>
    public partial class AppStudio : Application, IApplication
    {
        public AppStudio()
        {
            InitializeComponent();
        }

        public string AppName => nameof(AppStudio);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow window = new();
            window.Title = AppName;
            window.Show();
        }
    }
}
