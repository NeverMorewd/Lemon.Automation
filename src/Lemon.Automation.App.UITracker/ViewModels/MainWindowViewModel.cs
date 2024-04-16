using Lemon.Automation.App.UITracker.Track;
using R3;
using System.Diagnostics;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class MainWindowViewModel: IDisposable
    {
        private  readonly ElementTrackService _elementTracker;
        public MainWindowViewModel(ElementTrackService elementTracker) 
        {
            _elementTracker = elementTracker;
            IsTracking = new BindableReactiveProperty<bool>(false);
            SwitchTrackCommand = new ReactiveCommand<bool>(async (ischecked) => 
            {
                if (ischecked)
                {
                    var isTracking = await _elementTracker.Start();
                    IsTracking.Value = isTracking;
                }
                else
                {
                    await elementTracker.Stop();
                }
            });
            BrowseExeAndRunCommand = new ReactiveCommand<Unit>(u => 
            {
                Microsoft.Win32.OpenFileDialog dlg = new()
                {
                    Filter = "exe files (*.exe)|*.exe"
                };
                if (dlg.ShowDialog() == true)
                {
                    var filePath = dlg.FileName;
                    if (filePath.EndsWith(".exe") || filePath.EndsWith(".EXE"))
                    {
                        Process.Start(filePath);
                    }
                }
            });
        }



        public ReactiveCommand<bool> SwitchTrackCommand 
        { 
            get; 
        }
        public ReactiveCommand<Unit> BrowseExeAndRunCommand
        {
            get;
        }
        public BindableReactiveProperty<bool> IsTracking 
        { 
            get; 
        }

        public void Dispose()
        {
            Disposable.Combine(SwitchTrackCommand, IsTracking);
        }
    }
}
