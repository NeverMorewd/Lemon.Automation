using Lemon.Automation.App.UITracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Lemon.Automation.App.UITracker.Views
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : Page
    {
        public TestView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<TestViewMode>();
        }
    }
}
