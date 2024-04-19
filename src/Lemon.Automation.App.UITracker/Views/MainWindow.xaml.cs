using Lemon.Automation.App.UITracker.ViewModels;
using Lemon.Automation.Framework.Rx;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

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
