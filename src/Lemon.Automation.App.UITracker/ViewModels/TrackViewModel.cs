using Lemon.Automation.App.UITracker.Services;
using Lemon.Automation.Protos;
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
            SelectTypeCanUse = new BindableReactiveProperty<bool>(true);
            TrackType = new BindableReactiveProperty<TrackTypeEnum>(TrackTypeEnum.MouseMove);

            SwitchTrackCommand = new ReactiveCommand<bool>(async (ischecked) =>
            {
                if (ischecked)
                {
                    SelectTypeCanUse.Value = false;
                    var isTracking = await _elementTracker.Start(TrackType.Value);
                    SelectTypeCanUse.Value = !isTracking;
                    IsTracking.Value = isTracking;
                }
                else
                {
                    SelectTypeCanUse.Value = true;
                    await _elementTracker.Stop();
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

        public BindableReactiveProperty<bool> SelectTypeCanUse
        {
            get;
        }


        public BindableReactiveProperty<TrackTypeEnum> TrackType
        {
            get;
        }

        public void Dispose()
        {
            Disposable.Combine(SwitchTrackCommand, IsTracking);
        }
    }
}
