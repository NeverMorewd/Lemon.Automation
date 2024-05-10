using Lemon.Automation.App.UITracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using R3;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;

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
            DataContext = App.Current.Services.GetService<TestViewModel>();

            Observable.EveryValueChanged(this, _ => System.Windows.Forms.Cursor.Position)
                .ThrottleFirstFrame(60)
                .Subscribe(p => 
                {
                    Console.WriteLine($"point=({p.X},{p.Y})");
                });
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
