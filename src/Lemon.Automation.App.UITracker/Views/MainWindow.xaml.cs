using Lemon.Automation.App.UITracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using Lemon.Automation.Framework.Rx;

namespace Lemon.Automation.App.UITracker.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainWindowViewModel>();
            this.MakeDisposable();
        }
    }
}
