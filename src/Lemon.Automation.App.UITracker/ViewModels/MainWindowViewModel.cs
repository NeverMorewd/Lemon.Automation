using CommunityToolkit.Mvvm.Messaging;
using Gma.System.MouseKeyHook;
using Lemon.Automation.App.UITracker.Messages;
using Lemon.Automation.Framework.Rx;
using R3;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class MainWindowViewModel: RxViewModel
    {
        private readonly IKeyboardMouseEvents _keyboardMouseEvents;
        private readonly WeakReferenceMessenger _messenger;
        public MainWindowViewModel(IKeyboardMouseEvents hooks, 
            WeakReferenceMessenger messenger) 
        {
            AppName = App.Current.AppName ?? "";
            _keyboardMouseEvents = hooks;
            _messenger = messenger;
            _keyboardMouseEvents.KeyUp += KeyboardMouseEvents_KeyUp;
        }

        public string AppName 
        { 
            get; 
            private set; 
        }

        private void KeyboardMouseEvents_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _messenger.Send(new EscUpMessage(""));
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            _messenger.Cleanup();
            _keyboardMouseEvents.KeyUp -= KeyboardMouseEvents_KeyUp;
        }

    }
}
