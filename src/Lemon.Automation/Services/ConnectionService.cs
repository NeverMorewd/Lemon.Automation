using Lemon.Automation.CommandLines;
using Lemon.Automation.Domains;

namespace Lemon.Automation.Services
{
    public class ConnectionService : IConnection
    {
        public ConnectionService(CommandOptions options) 
        {
            ConnectionKey = options.ConnectionKey;
        }
        public string? ConnectionKey
        {
            get; 
            private set;
        }
    }
}
