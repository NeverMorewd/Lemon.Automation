using Lemon.Automation.App.UITracker.ViewModels;
using Wpf.Ui.Controls;

namespace Lemon.Automation.App.UITracker.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
