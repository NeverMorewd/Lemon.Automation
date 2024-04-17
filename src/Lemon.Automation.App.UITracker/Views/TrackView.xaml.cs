using Lemon.Automation.App.UITracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Lemon.Automation.App.UITracker.Views
{
    /// <summary>
    /// Interaction logic for TrackView.xaml
    /// </summary>
    public partial class TrackView : Page
    {
        public TrackView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<TrackViewModel>();
        }
    }
}
