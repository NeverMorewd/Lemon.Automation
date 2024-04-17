using Lemon.Automation.Framework.Rx;
using R3;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class MainWindowViewModel: RxViewModel
    {
        public MainWindowViewModel() 
        {
            AppName = "Lemon.UIA.Tracker";
        }

        public string AppName 
        { 
            get; 
            private set; 
        }
    }
}
