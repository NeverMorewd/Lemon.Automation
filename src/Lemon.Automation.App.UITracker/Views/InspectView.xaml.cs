using Lemon.Automation.App.UITracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Lemon.Automation.App.UITracker.Views
{
    /// <summary>
    /// Interaction logic for InspectView.xaml
    /// </summary>
    public partial class InspectView : Page
    {
        public InspectView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<InspectViewModel>();
        }
    }
}
