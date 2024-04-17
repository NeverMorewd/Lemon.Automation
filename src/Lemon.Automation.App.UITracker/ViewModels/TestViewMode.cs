using Lemon.Automation.Framework.Rx;
using R3;
using System.Diagnostics;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class TestViewMode : RxViewModel
    {
        public TestViewMode()
        {
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

        public ReactiveCommand<Unit> BrowseExeAndRunCommand
        {
            get;
        }

        public override void Dispose()
        {
            Disposable.Combine(BrowseExeAndRunCommand).Dispose();
        }
    }
}
