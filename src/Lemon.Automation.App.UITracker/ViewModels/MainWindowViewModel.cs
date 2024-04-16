using Lemon.Automation.App.UITracker.Track;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        }



        public ReactiveCommand<bool> SwitchTrackCommand { get; }
        public BindableReactiveProperty<bool> IsTracking { get; }

        public void Dispose()
        {
            Disposable.Combine(SwitchTrackCommand, IsTracking);
        }
    }
}
