using Lemon.Automation.App.UITracker.Track;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class MainWindowViewModel: IDisposable
    {
        private  readonly ElementTrackService _elementTracker;
        public MainWindowViewModel(ElementTrackService elementTracker) 
        {
            _elementTracker = elementTracker;
            StartTracking = new ReactiveCommand<Unit>(async (unit) => 
            {
               await elementTracker.Start(CancellationToken.None);
            });
        }



        public ReactiveCommand<Unit> StartTracking { get; }

        public void Dispose()
        {
            Disposable.Combine(StartTracking);
        }
    }
}
