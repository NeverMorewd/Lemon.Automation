using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Lemon.Automation.App.UITracker.Messages
{
    public class EscUpMessage : ValueChangedMessage<string>
    {
        public EscUpMessage(string source) : base(source)
        {

        }
    }
}
