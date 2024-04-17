using Lemon.Automation.App.UITracker.Services;
using R3;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class TrackViewModel : IDisposable
    {
        private readonly ElementTrackService _elementTracker;

        public TrackViewModel(ElementTrackService elementTracker)
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

        public ReactiveCommand<bool> SwitchTrackCommand
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
