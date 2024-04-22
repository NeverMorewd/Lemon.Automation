using Lemon.Automation.App.UITracker.ViewModels;
using Lemon.Automation.Framework.Rx;
using Microsoft.Extensions.DependencyInjection;

namespace Lemon.Automation.App.UITracker.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : RxPage
    {
        public HomeView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<HomeViewModel>();
        }
    }
}
